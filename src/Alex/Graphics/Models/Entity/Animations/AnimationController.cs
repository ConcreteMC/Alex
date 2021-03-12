using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Alex.Entities;
using Alex.Items;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class AnimationController
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(AnimationController));

		public bool Initialized => _didInit;
		private Queue<ModelBoneAnimation> AnimationQueue { get; }
		private Entities.Entity Entity { get; }
		private MoLangRuntime Runtime { get; set; }
		public AnimationController(Entities.Entity entity)
		{
			Entity = entity;
			AnimationQueue = new Queue<ModelBoneAnimation>();
			Runtime = new MoLangRuntime();
		}

		private IExpression[] _preRenderExpressions = null;
		private EntityDescription _entityDefinition = null;
		private IReadOnlyDictionary<string, AnimationEntry> _animations = null;
		private object _lock = new object();

		private AnimationState _animationState = null;
		private static readonly Regex ControllerRegex = new Regex("", RegexOptions.Compiled);

		private EntityQueryStruct _queryStruct = null;
		public void UpdateEntityDefinition(EntityDescription definition)
		{
			//Monitor.Enter(_lock);

			try
			{
				List<IExpression> preRender = new List<IExpression>();
				
				MoLangRuntime runtime = new MoLangRuntime();
				_context.Clear();
				//SetEnvironment(runtime.Environment);
				runtime.Environment.Structs.TryAdd("query", _queryStruct = new EntityQueryStruct(Entity));

				if (definition.Scripts != null)
				{
					foreach (var init in ConditionalExecute(runtime, definition.Scripts.Initialize, _context)) { }

					foreach (var list in definition.Scripts.PreAnimation)
					{
						preRender.AddRange(list);
					}
				}

				Dictionary<string, AnimationEntry> animations = new Dictionary<string, AnimationEntry>();

				var resources = Alex.Instance.Resources.BedrockResourcePack;

				if (definition.Animations != null)
				{
					foreach (var kv in definition.Animations)
					{
						if (kv.Value.StartsWith("controller."))
						{
							if (resources.AnimationControllers.TryGetValue(kv.Value, out var controller))
								animations.TryAdd(kv.Key, new AnimationEntry(controller));
						}
						else
						{
							if (resources.Animations.TryGetValue(kv.Value, out var animation))
								animations.TryAdd(kv.Key, new AnimationEntry(animation));
						}

						/*else if (resources.Animations.TryGetValue(kv.Value, out var animation))
						{
							animations.TryAdd(kv.Key, animation);
						}*/
					}
				}

				_animations = animations;
				_entityDefinition = definition;
				_preRenderExpressions = preRender.ToArray();
				Runtime = runtime;

				_didInit = true;
			}
			finally
			{
			//	Monitor.Exit(_lock);
			}
		}

		private class AnimationEntry
		{
			public ResourcePackLib.Json.Bedrock.Entity.AnimationController Controller { get; }
			public Animation Animation { get; }
			
			public string State { get; set; } = null;
			
			public AnimationEntry(ResourcePackLib.Json.Bedrock.Entity.AnimationController controller)
			{
				Controller = controller;
				Animation = null;

				State = controller.InitialState;
			}
			
			public AnimationEntry(Animation animation)
			{
				Controller = null;
				Animation = animation;
			}
		}

		private IEnumerable<IMoValue> ConditionalExecute(MoLangRuntime runtime, IExpression[][] expressions, IDictionary<string, IMoValue> context)
		{
			if (expressions == null)
				yield break;
			
			foreach (var expressionList in expressions)
			{
				if (expressionList == null || expressionList.Length == 0)
					continue;
				
				yield return runtime.Execute(expressionList, context);
			}
		}

		private Stopwatch _deltaTimeStopwatch = new Stopwatch();
		private bool _didInit = false;

		private IDictionary<string, IMoValue> _context = new Dictionary<string, IMoValue>();
		private void DoImportant()
		{
			if (!_didInit)
				return;
			
			//if (!Monitor.TryEnter(_lock, 0))
			//	return;

			var renderer = Entity?.ModelRenderer;

			if (renderer == null)
				return;
			
			try
			{
				var runtime = Runtime;

				if (runtime == null)
					return;

				var def = _entityDefinition;

				if (def == null)
					return;

				var animations = _animations;

				if (animations == null)
					return;
				
				runtime.Environment.SetValue("variable.gliding_speed_value", new DoubleValue(1d));
				runtime.Environment.SetValue("variable.is_first_person", new DoubleValue(Entity.IsFirstPersonMode ? 1 : 0));
				runtime.Environment.SetValue("variable.attack_time", new DoubleValue(Entity.AttackTime));
				runtime.Environment.SetValue("variable.is_using_vr", new DoubleValue(0d));
				runtime.Environment.SetValue("variable.is_paperdoll", new DoubleValue(0d));
				runtime.Environment.SetValue("variable.swim_amount", Entity.IsSwimming ? new DoubleValue(1d) : new DoubleValue(0d));
				
				_context.Clear();
				
				if (_preRenderExpressions != null)
					runtime.Execute(_preRenderExpressions, _context);

				if (def.Scripts != null)
				{
					if (def.Scripts.ShouldUpdateBonesAndEffectsOffscreen != null)
					{
						if (!Entity.IsRendered && !runtime.Execute(
							def.Scripts.ShouldUpdateBonesAndEffectsOffscreen, _context).AsBool())
							return;
					}
					
					foreach (var key in def.Scripts.Animate)
					{
						HandleAnnoyingMolangElement(runtime, key, _context);
					}
				}

				renderer.ApplyPending();
			}
			finally
			{
			//	Monitor.Exit(_lock);
			}
		}

		private void HandleAnnoyingMolangElement(MoLangRuntime runtime, AnnoyingMolangElement key, IDictionary<string, IMoValue> context)
		{
			if (key.IsString)
			{
				TryAnimate(runtime, key.StringValue, context);
			}
			else
			{
				foreach (var expression in key.Expressions)
				{
					if (runtime.Execute(expression.Value, context).AsBool())
					{
						TryAnimate(runtime, expression.Key, context);
					}
				}
			}
		}
		
		private void TryAnimate(MoLangRuntime runtime, string name, IDictionary<string, IMoValue> context)
		{
			var renderer = Entity.ModelRenderer;

			if (renderer == null)
				return;
			
			if (_animations.TryGetValue(name, out var animation))
			{
				if (animation == null)
					return;

				if (animation.Controller != null)
				{
					var controller = animation.Controller;
					string state = animation.State;

					if (string.IsNullOrWhiteSpace(state))
						return;

					if (controller.States.TryGetValue(state, out var animationState))
					{
						if (animationState.Variables != null)
						{
							foreach (var anim in animationState.Variables)
							{
								if (anim.Value.Input != null)
								{
									double input = runtime.Execute(anim.Value.Input, _context).AsDouble();
									/*double start = 0d;
									double end = 1d;
									if (anim.Value.RemapCurve != null)
									{
										for (int i = 0; i < anim.Value.RemapCurve.Count - 1; i += 2)
										{
											//var s = anim.Value.RemapCurve.
											if (input >= map.Key)
											{
												
											}
										}
										//start = anim.Value.RemapCurve.
									}*/
									
									runtime.Environment.Structs["variable"].Set(
										anim.Key, new DoubleValue(input));
								}
								//HandleAnnoyingMolangElement(runtime, anim, context);
							}
						}
						
						if (animationState.Animations != null)
						{
							foreach (var anim in animationState.Animations)
							{
								HandleAnnoyingMolangElement(runtime, anim, context);
							}
						}

						if (animationState.Transitions != null)
						{
							var oldState = animation.State;
							bool stateUpdated = false;
							foreach (var transition in animationState.Transitions)
							{
								foreach (var expression in transition.Expressions)
								{
									var result = runtime.Execute(expression.Value, context);
									if (result.AsBool())
									{
									//	Console.WriteLine($"Old={oldState} New={expression.Key} (Cause: {result.Value})");	
										
										animation.State = expression.Key;
										stateUpdated = true;
										break;
									}
								}

								if (stateUpdated)
									break;
							}
						}
					}
				}
				else if (animation.Animation != null)
				{
					var anim = animation.Animation;

					if (anim.Bones == null)
						return;
					
					foreach (var bone in anim.Bones)
					{
						if (bone.Value == null) continue;

						if (renderer.GetBone(bone.Key, out var modelBone))
						{
							var value = bone.Value;
						
							//var rotationOutput = ConditionalExecute(runtime, value.Rotation, context).ToArray();
							//var positionOutputs = ConditionalExecute(runtime, value.Position, context).ToArray();
							//var scaleOutputs = ConditionalExecute(runtime, value.Scale, context).ToArray();
							
							var targetRotation = value.Rotation?.Evaluate(runtime, Vector3.Zero) ?? Vector3.Zero;
							var targetPosition = value.Position?.Evaluate(runtime, modelBone.Position) ?? Vector3.Zero;// GetVector3(Vector3.Zero, positionOutputs);
							var targetScale = value.Scale?.Evaluate(runtime, modelBone.Scale) ?? Vector3.Zero;//GetVector3(Vector3.Zero, scaleOutputs);

							modelBone.MoveOverTime(
								targetPosition, targetRotation * new Vector3(-1f, 1f, 1f), targetScale,
								_deltaTimeStopwatch.Elapsed, anim.OverridePreviousAnimation,
								anim.BlendWeight != null ? runtime.Execute(anim.BlendWeight).AsFloat() : 1f);

							//	modelBone.Rotation = GetVector3(modelBone.Rotation, rotationOutput);
							//modelBone.Position = GetVector3(modelBone.Position, positionOutputs);
						}
					}
				}
			}
			else
			{
				Log.Debug($"Missing animation: {name}");
			}
		}

		public void OnTick()
		{
			_deltaTimeStopwatch.Stop();
			
			_queryStruct?.Tick(_deltaTimeStopwatch.Elapsed);
			DoImportant();
			
			_deltaTimeStopwatch.Restart();
		}
	}
}