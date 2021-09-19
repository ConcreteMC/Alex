using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Alex.Common.World;
using Alex.Entities;
using Alex.Entities.Components;
using Alex.Items;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Abstraction;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Microsoft.Xna.Framework;
using NLog;
using Org.BouncyCastle.Utilities;
using RocketUI.Input;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class AnimationComponent : EntityComponent, ITicked
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(AnimationComponent));

		public bool Initialized => _didInit;
		public MoLangRuntime Runtime { get; set; }
		public AnimationComponent(Entities.Entity entity) : base(entity)
		{
			Runtime = new MoLangRuntime();
		}

		private IExpression[] _preRenderExpressions = null;
		private EntityDescription _entityDefinition = null;
		private IReadOnlyDictionary<string, IAnimation> _animations = null;

		public readonly IDictionary<string, IMoValue> Context = new Dictionary<string, IMoValue>(StringComparer.OrdinalIgnoreCase);
		public void UpdateEntityDefinition(IAnimationProvider animationProvider, EntityDescription definition)
		{
			//Monitor.Enter(_lock);

			try
			{
				List<IExpression> preRender = new List<IExpression>();

				MoLangRuntime runtime = new MoLangRuntime();
				Context.Clear();
				//SetEnvironment(runtime.Environment);
				runtime.Environment.Structs.TryAdd("query", new ObjectStruct(Entity));

				if (definition.Scripts != null)
				{
					if (definition.Scripts.Scale != null)
					{
						var scale = runtime.Execute(definition.Scripts.Scale, Context);

						if (scale is DoubleValue dv)
						{
							//Entity.Scale = dv.AsFloat();
						}
					}

					if (definition.Scripts.Initialize != null)
					{
						foreach (var init in ConditionalExecute(runtime, definition.Scripts.Initialize, Context))
						{
							
						}
					}

					if (definition.Scripts.PreAnimation != null)
					{
						foreach (var list in definition.Scripts.PreAnimation)
						{
							preRender.AddRange(list);
						}
					}
				}
				
				if (definition.Animations != null)
					LoadAnimations(animationProvider, definition.Animations);

				if (definition.RenderControllers != null)
					LoadRenderControllers(animationProvider, definition.RenderControllers);
				
				if (definition.AnimationControllers != null)
					LoadAnimationControllers(animationProvider, definition.AnimationControllers);
				
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

		private IReadOnlyDictionary<string, IAnimation> _animationControllers;
		private void LoadAnimationControllers(IAnimationProvider animationProvider, AnnoyingMolangElement[] input)
		{
			Dictionary<string, IAnimation> animationControllers =
				new Dictionary<string, IAnimation>(StringComparer.OrdinalIgnoreCase);

			int idx = 0;
			foreach (var element in input)
			{
				idx++;
				string key = null;
				string search = null;
				
				if (!element.IsString)
				{
					foreach (var kv in element.Expressions)
					{
						IMoValue result = null;

						if (kv.Value.Length == 1)
						{
							if (kv.Value[0] is NameExpression ne)
								search = ne.Name.ToString();
						}
						else
						{
							result = Execute(kv.Value);
						}

						if (result is StringValue sv)
						{
							key = kv.Key;
							search = sv.Value;
						}
					}
				}
				
				if (search == null)
					continue;

				if (key == null)
				{
					key = $"anim{idx}";
				}
				
				if (animationProvider.TryGetAnimationController(search, out var controller))
				{
					if (!animationControllers.TryAdd(key, new AnimationController(this, controller)))
					{
						Log.Warn($"Failed to add animation controller: {search}");
					}
				}
			}

			_animationControllers = animationControllers;
		}

		private void LoadRenderControllers(IAnimationProvider animationProvider, AnnoyingMolangElement[] input)
		{
			
		}

		private void LoadAnimations(IAnimationProvider animationProvider, Dictionary<string, string> input)
		{
			Dictionary<string, IAnimation> animations = new Dictionary<string, IAnimation>(StringComparer.OrdinalIgnoreCase);
			foreach (var kv in input)
			{
				if (kv.Value.StartsWith("controller."))
				{
					if (animationProvider.TryGetAnimationController(kv.Value, out var controller))
					{
						if (!animations.TryAdd(kv.Key, new AnimationController(this, controller)))
						{
							Log.Warn($"Failed to add animation controller: {kv.Key}");
						}
					}
				}
				else
				{
					if (animationProvider.TryGetAnimation(kv.Value, out var animation))
					{
						var entityAnimation = new EntityAnimation(this, animation, kv.Key);
						//entityAnimation.Play();
						
						if (!animations.TryAdd(kv.Key, entityAnimation))
						{
							Log.Warn($"Failed to add animation: {kv.Key}");
						}
					}
				}
			}
			_animations = animations;
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
		private ModelRenderer _modelRenderer = null;
		private void ProcessAnimations()
		{
			if (!_didInit)
				return;
			
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
				
				if (renderer != _modelRenderer)
				{
					foreach (var anim in _animations)
					{
						anim.Value.UpdateBindings(renderer);
					}
					_modelRenderer = renderer;
				}
				
				Context.Clear();
				
				if (_preRenderExpressions != null)
					runtime.Execute(_preRenderExpressions, Context);
				
				runtime.Environment.SetValue("variable.gliding_speed_value", new DoubleValue(1d));
				runtime.Environment.SetValue("variable.is_first_person", new DoubleValue(Entity.IsFirstPersonMode ? 1 : 0));
				runtime.Environment.SetValue("variable.attack_time", new DoubleValue(Entity.AttackTime));
				runtime.Environment.SetValue("variable.is_using_vr", new DoubleValue(0d));
				runtime.Environment.SetValue("variable.is_paperdoll", new DoubleValue(0d));
				runtime.Environment.SetValue("variable.swim_amount", Entity.IsSwimming ? new DoubleValue(1d) : new DoubleValue(0d));
				runtime.Environment.SetValue("variable.bob_animation", new DoubleValue(1d));
				runtime.Environment.SetValue("variable.hand_bob", new DoubleValue(1d));
				
				if (Entity is RemotePlayer player)
				{
					bool holdingLeft = false;
					bool holdingRight = false;

					var leftHand = player.Inventory?.OffHand;

					if (leftHand != null && !(leftHand is ItemAir) && leftHand.Count > 0)
					{
						holdingLeft = true;
					}
					
					var rightHand = player.Inventory?.MainHand;

					if (rightHand != null && !(rightHand is ItemAir) && rightHand.Count > 0)
					{
						holdingRight = true;
					}
					
					runtime.Environment.SetValue(
						"variable.is_holding_left",
						new DoubleValue(holdingLeft));

					runtime.Environment.SetValue(
						"variable.is_holding_right",
						new DoubleValue(holdingRight));
					
					runtime.Environment.SetValue("variable.player_x_rotation", new DoubleValue(Entity.KnownPosition.Pitch));
				}

				if (def.Scripts != null)
				{
					if (def.Scripts.ShouldUpdateBonesAndEffectsOffscreen != null)
					{
						if (!Entity.IsRendered && !runtime.Execute(
							def.Scripts.ShouldUpdateBonesAndEffectsOffscreen, Context).AsBool())
							return;
					}

					if (def.Scripts.Animate != null && def.Scripts.Animate.Length > 0)
					{
						foreach (var key in def.Scripts.Animate)
						{
							ExecuteAnnoying(key);
						}
					}
				}

				if (_animationControllers != null)
				{
					foreach (var controller in _animationControllers)
					{
						controller.Value.Tick();
					}
				}
				
				renderer.ApplyPending();
			}
			finally
			{
			//	Monitor.Exit(_lock);
			}
		}

		public IMoValue Execute(IExpression[] expressions)
		{
			return Runtime.Execute(expressions, Context);
		}

		public void ExecuteAnnoying(AnnoyingMolangElement key, bool forceStop = false)
		{
			if (key.IsString)
			{
				ExecuteAnimationUpdate(key.StringValue, !forceStop);
			}
			else
			{
				foreach (var expression in key.Expressions)
				{
					ExecuteAnimationUpdate(expression.Key,  !forceStop && Execute(expression.Value).AsBool());
				}
			}
		}

		//private Stopwatch _animationRunTime = new Stopwatch();
		internal void ExecuteAnimationUpdate(string name, bool play)
		{
			if (_animations.TryGetValue(name, out var animation))
			{
				if (animation is EntityAnimation ea)
				{
					if (ea.Playing || play)
					{
						if (!ea.CanPlay() || !play)
						{
							ea.Stop();
							return;
						}
						
						if (!ea.Playing)
							ea.Play();
					
						ea.Tick();

						ea.AfterUpdate();
					}
				}
				else
				{
					animation.Tick();
				}
			}
			else
			{
				Log.Debug($"Could not find animation: {name}");
			}
		}
		
		internal bool TryGetAnimation(string name, out EntityAnimation animation)
		{
			animation = null;
			if (_animations.TryGetValue(name, out var anim) && anim is EntityAnimation entityAnimation)
			{
				animation = entityAnimation;

				return true;
			}

			return false;
		}

		/// <inheritdoc />
		void ITicked.OnTick()
		{
			if (!Enabled)
				return;
			
			_deltaTimeStopwatch.Stop();
			
			//_queryStruct?.Tick(_deltaTimeStopwatch.Elapsed);
			ProcessAnimations();
			
			_deltaTimeStopwatch.Restart();
		}
	}
}