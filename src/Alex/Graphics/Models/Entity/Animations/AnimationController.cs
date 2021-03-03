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

		private List<IExpression> _preRenderExpressions = null;
		private EntityDescription _entityDefinition = null;
		private IReadOnlyDictionary<string, AnimationEntry> _animations = null;
		private object _lock = new object();

		private AnimationState _animationState = null;
		private static readonly Regex ControllerRegex = new Regex("", RegexOptions.Compiled);
		public void UpdateEntityDefinition(EntityDescription definition)
		{
			//Monitor.Enter(_lock);

			try
			{
				List<IExpression> preRender = new List<IExpression>();
				
				MoLangRuntime runtime = new MoLangRuntime();
				//SetEnvironment(runtime.Environment);
				runtime.Environment.Structs.TryAdd("query", InitQueryStruct());

				if (definition.Scripts != null)
				{
					foreach (var init in ConditionalExecute(runtime, definition.Scripts.Initialize)) { }

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
				_preRenderExpressions = preRender;
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

		private IEnumerable<IMoValue> ConditionalExecute(MoLangRuntime runtime, List<IExpression>[] expressions)
		{
			if (expressions == null)
				yield break;
			
			foreach (var expressionList in expressions)
			{
				if (expressionList == null || expressionList.Count == 0)
					continue;
				
				yield return runtime.Execute(expressionList);
			}
		}

		private QueryStruct InitQueryStruct()
		{
			Dictionary<string, Func<MoParams, object>> q = new Dictionary<string, Func<MoParams, object>>();
			q.Add("life_time", mo => new DoubleValue(Entity.LifeTime.TotalSeconds));
			q.Add("position_delta", mo =>
			{
				var delta = Entity.KnownPosition;
				double amount = 0d;
				switch (mo.GetInt(0))
				{
					case 0: //X-Axis
						
						break;
					case 1: //Y-Axis
						
						break;
					case 2: //Z-Axis
						
						break;
				}
				
				return new DoubleValue(amount);
			});
			q.Add("position", mo =>
			{
				double amount = 0d;
				switch (mo.GetInt(0))
				{
					case 0: //X-Axis
						amount = Entity.KnownPosition.X;
						break;
					case 1: //Y-Axis
						amount = Entity.KnownPosition.Y;
						break;
					case 2: //Z-Axis
						amount = Entity.KnownPosition.Z;
						break;
				}
				
				return new DoubleValue(amount);
			});
			
			q.Add("main_hand_item_use_duration", NoOp);
			q.Add("main_hand_item_max_duration", NoOp);
			
			q.Add("modified_distance_moved", mo =>
			{
				return new DoubleValue(Entity.Movement.DistanceMoved);
			});
			
			q.Add("modified_move_speed", mo =>
			{
			//	Console.WriteLine($"Movement: {Entity.MovementSpeed:F3} | Current: {Entity.CurrentSpeed:F3}");
			//	Console.WriteLine(Entity.CurrentSpeed);
				return new DoubleValue((1f / (Entity.CalculateMovementSpeed() * 43f)) * (Entity.Movement.MetersPerSecond));
			});
			
			q.Add("delta_time", mo =>
			{
				return new DoubleValue(_deltaTimeStopwatch.Elapsed.TotalSeconds);
			});
			
			q.Add("get_equipped_item_name", mo =>
			{
				bool isOffHand = false;
				if (mo.Contains(0))
				{
					var firstArgument = mo.Get(0);

					if (firstArgument is StringValue sv)
					{
						if (!sv.Value.Equals("main_hand"))
							isOffHand = true;
					}
					else if (firstArgument is DoubleValue dv)
					{
						if (dv.Value > 0)
							isOffHand = true;
					}
				}
				
				if (mo.Contains(1))
				{
					
				}

				Item item = null;
				if (!isOffHand)
					item = Entity.Inventory.MainHand;
				else
					item = Entity.Inventory.OffHand;

				if (item?.Name == null)
					return new StringValue("air");
				
				return new StringValue(item.Name.Replace("minecraft:", ""));
			});
			
			q.Add("ground_speed", mo =>
			{
				return new DoubleValue(Entity.Movement.MetersPerSecond);
			});
			
			q.Add("log", mo =>
			{
				Log.Debug($"Animation Debug: {string.Join(", ", mo.GetParams().Select(x => x.AsString()))}");
				return null;
			});
			
			q.Add("vertical_speed", mo => new DoubleValue(Entity.Movement.VerticalSpeed));
			q.Add("time_of_day", mo => new DoubleValue((1f / 24000f) * Entity.Level.TimeOfDay));

			q.Add("is_alive", mo => Entity.HealthManager.Health > 0 ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_on_ground", mo => Entity.KnownPosition.OnGround ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_riding", mo => Entity.IsRiding ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_sneaking", mo => Entity.IsSneaking ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_charging", mo => Entity.IsAngry ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_tamed", mo => Entity.IsTamed ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_using_item", mo => Entity.IsUsingItem ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_wall_climbing", mo => Entity.IsWallClimbing ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_onfire", mo => Entity.IsOnFire ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_on_fire", mo => Entity.IsOnFire ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_sprinting", mo => Entity.IsSprinting ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_on_screen", mo => Entity.IsRendered ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_moving", mo => Entity.IsMoving ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_leashed", mo => Entity.IsLeashed ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_interested", mo => Entity.IsInterested ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_in_love", mo => Entity.IsInLove ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_ignited", mo => Entity.IsIgnited ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_eating", mo => Entity.IsEating ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_chested", mo => Entity.IsChested ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_breathing", mo => Entity.IsBreathing ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_baby", mo => Entity.IsBaby ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_stackable", mo => Entity.IsStackable ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_in_water", mo => Entity.IsInWater ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_in_water_or_rain", mo => (Entity.IsInWater || Entity.Level.Raining) ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_sheared", mo => Entity.IsSheared ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_silent", mo => Entity.IsSilent ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_powered", mo => Entity.IsPowered ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_elder", mo => Entity.IsElder ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_resting", mo => Entity.IsResting ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_tempted", mo => Entity.IsTempted ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_critical", mo => Entity.IsCritical ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_converting", mo => Entity.IsConverting ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_standing", mo => (Entity.IsStanding) ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_sleeping", mo => Entity.IsSleeping ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_swimming", mo => Entity.IsSwimming ? DoubleValue.One : DoubleValue.Zero);
			q.Add("is_sitting", mo => Entity.IsSitting ? DoubleValue.One : DoubleValue.Zero);
			q.Add("has_gravity", mo => Entity.IsAffectedByGravity ? DoubleValue.One : DoubleValue.Zero);
			q.Add("has_collision", mo => Entity.HasCollision ? DoubleValue.One : DoubleValue.Zero);
			q.Add("can_fly", mo => Entity.CanFly ? DoubleValue.One : DoubleValue.Zero);

			return new QueryStruct(q);
		}

		private IMoValue NoOp(MoParams mo)
		{
			return new DoubleValue(0);
		}

		private Stopwatch _deltaTimeStopwatch = new Stopwatch();
		private bool _didInit = false;
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
				runtime.Environment.SetValue("variable.swim_amount", new DoubleValue(0d));
				
				if (_preRenderExpressions != null)
					runtime.Execute(_preRenderExpressions);

				if (def.Scripts != null)
				{
					foreach (var key in def.Scripts.Animate)
					{
						HandleAnnoyingMolangElement(runtime, key);
					}
				}

				renderer.ApplyPending();
			}
			finally
			{
			//	Monitor.Exit(_lock);
			}
		}

		private void HandleAnnoyingMolangElement(MoLangRuntime runtime, AnnoyingMolangElement key)
		{
			if (key.IsString)
			{
				TryAnimate(runtime, key.StringValue);
			}
			else
			{
				foreach (var expression in key.Expressions)
				{
					if (runtime.Execute(expression.Value).AsDouble() > 0)
					{
						TryAnimate(runtime, expression.Key);
					}
				}
			}
		}
		
		private void TryAnimate(MoLangRuntime runtime, string name)
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
						if (animationState.Animations != null)
						{
							foreach (var anim in animationState.Animations)
							{
								HandleAnnoyingMolangElement(runtime, anim);
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
									var result = runtime.Execute(expression.Value);
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
						
							var rotationOutput = ConditionalExecute(runtime, value.Rotation).ToArray();
							var positionOutputs = ConditionalExecute(runtime, value.Position).ToArray();
							var scaleOutputs = ConditionalExecute(runtime, value.Scale).ToArray();
							
							var targetRotation = GetVector3(Vector3.Zero, rotationOutput);
							var targetPosition = GetVector3(Vector3.Zero, positionOutputs);
							var targetScale = GetVector3(Vector3.Zero, scaleOutputs);
							
							modelBone.MoveOverTime(targetPosition, targetRotation * new Vector3(-1f, 1f, 1f), targetScale, _deltaTimeStopwatch.Elapsed);
							//	modelBone.Rotation = GetVector3(modelBone.Rotation, rotationOutput);
							//modelBone.Position = GetVector3(modelBone.Position, positionOutputs);
						}
					}
				}
			}
			else
			{
				Log.Warn($"Missing animation: {name}");
			}
		}

		private Vector3 GetVector3(Vector3 currentValue, IMoValue[] values)
		{
			if (values.Length == 0)
				return currentValue;
			
			float x = currentValue.X, y = currentValue.Y, z = currentValue.Z;
							
			if (values.Length == 3)
			{
				x += values[0].AsFloat();
				y += values[1].AsFloat();
				z += values[2].AsFloat();
			}else if (values.Length == 1)
			{
				x += y += values[0].AsFloat();
				z += values[0].AsFloat();
			}

			return new Vector3(float.IsNaN(x) ? 0 : x, float.IsNaN(y) ? 0 : y, float.IsNaN(z) ? 0 : z);
		}
		
		public void OnTick()
		{
			_deltaTimeStopwatch.Stop();
			DoImportant();
			_deltaTimeStopwatch.Restart();
		}
	}
}