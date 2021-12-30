using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Alex.Common.World;
using Alex.Entities;
using Alex.Entities.Components;
using Alex.Entities.Passive;
using Alex.Items;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;
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
		public EntityDescription EntityDefinition { get; private set; }
		private IReadOnlyDictionary<string, IAnimation> _animations = null;
		private IReadOnlyDictionary<string, EntityRenderController> _renderControllers = null;
		
		public readonly IDictionary<string, IMoValue> Context = new Dictionary<string, IMoValue>(StringComparer.OrdinalIgnoreCase);
		public void UpdateEntityDefinition(IAnimationProvider animationProvider, IRenderControllerProvider controllerProvider, EntityDescription definition)
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
					LoadRenderControllers(controllerProvider, definition.RenderControllers);
				
				if (definition.AnimationControllers != null)
					LoadAnimationControllers(animationProvider, definition.AnimationControllers);
				
				EntityDefinition = definition;
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
								search = ne.Name.Path.ToString();
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
				else
				{
					search = element.StringValue;
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

		private void LoadRenderControllers(IRenderControllerProvider renderControllerProvider, AnnoyingMolangElement[] input)
		{
			Dictionary<string, EntityRenderController> renderControllers =
				new Dictionary<string, EntityRenderController>(StringComparer.OrdinalIgnoreCase);
			
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
								search = ne.Name.Path.ToString();
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
				else
				{
					search = element.StringValue;
				}
				
				if (search == null)
					continue;

				if (key == null)
				{
					key = $"renderer{idx}";
				}
				
				if (renderControllerProvider.TryGetRenderController(search, out var controller))
				{
					if (!renderControllers.TryAdd(key, new EntityRenderController(this, controller)))
					{
						Log.Warn($"Failed to add RenderController: {search}");
					}
				}
			}

			_renderControllers = renderControllers;
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

		private static readonly MoPath _glidingSpeed = new MoPath("variable.gliding_speed_value");
		private static readonly MoPath _isFirstPerson = new MoPath("variable.is_first_person");
		private static readonly MoPath _attackTime = new MoPath("variable.attack_time");
		private static readonly MoPath _isUsingVr = new MoPath("variable.is_using_vr");
		private static readonly MoPath _isPaperDoll = new MoPath("variable.is_paperdoll");
		private static readonly MoPath _swimAmount = new MoPath("variable.swim_amount");
		private static readonly MoPath _bobAnimation = new MoPath("variable.bob_animation");
		private static readonly MoPath _handBob = new MoPath("variable.hand_bob");
		private static readonly MoPath _isHoldingLeft = new MoPath("variable.is_holding_left");
		private static readonly MoPath _isHoldingRight = new MoPath("variable.is_holding_right");
		private static readonly MoPath _playerXRotation = new MoPath("variable.player_x_rotation");
		
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

				var def = EntityDefinition;

				if (def == null)
					return;

				var animations = _animations;

				var renderControllers = _renderControllers;
				if (animations == null)
					return;
				
				if (renderer != _modelRenderer)
				{
					foreach (var anim in animations)
					{
						anim.Value.UpdateBindings(renderer);
					}

					foreach (var controller in renderControllers)
					{
						controller.Value.UpdateBindings(renderer);
					}
					_modelRenderer = renderer;
				}
				
				Context.Clear();
				
				if (_preRenderExpressions != null)
					runtime.Execute(_preRenderExpressions, Context);
				
				runtime.Environment.SetValue(_glidingSpeed, new DoubleValue(1d));
				runtime.Environment.SetValue(_isFirstPerson, new DoubleValue(Entity.IsFirstPersonMode ? 1 : 0));
				runtime.Environment.SetValue(_attackTime, new DoubleValue(Entity.AttackTime));
				runtime.Environment.SetValue(_isUsingVr, new DoubleValue(0d));
				runtime.Environment.SetValue(_isPaperDoll, new DoubleValue(0d));
				runtime.Environment.SetValue(_swimAmount, Entity.IsSwimming ? new DoubleValue(1d) : new DoubleValue(0d));
				runtime.Environment.SetValue(_bobAnimation, new DoubleValue(1d));
				runtime.Environment.SetValue(_handBob, new DoubleValue(1d));
				
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
						_isHoldingLeft,
						new DoubleValue(holdingLeft));

					runtime.Environment.SetValue(
						_isHoldingRight,
						new DoubleValue(holdingRight));
					
					runtime.Environment.SetValue(_playerXRotation, new DoubleValue(Entity.KnownPosition.Pitch));
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

				if (_renderControllers != null)
				{
					foreach (var controller in _renderControllers)
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

		public void InvokeRenderControllerUpdate()
		{
			if (_renderControllers != null)
			{
				foreach (var controller in _renderControllers)
				{
					controller.Value.ForceUpdate();
				}
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
				if (!string.Equals(name, "first_person.breathing_bob"))
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
			//Stopwatch sw = Stopwatch.StartNew();
			//_queryStruct?.Tick(_deltaTimeStopwatch.Elapsed);
			ProcessAnimations();
			//sw.Stop();
			//if (sw.Elapsed.TotalMilliseconds > 10)
			
			_deltaTimeStopwatch.Restart();
		}
	}
}