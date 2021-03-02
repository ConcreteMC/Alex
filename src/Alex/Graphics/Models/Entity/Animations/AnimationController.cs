using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models.Entity.Animations
{
	public class AnimationController
	{
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
		private IReadOnlyDictionary<string, Animation> _animations = null;
		private object _lock = new object();
		public void UpdateEntityDefinition(EntityDescription definition)
		{
			Monitor.Enter(_lock);

			try
			{
				MoLangRuntime runtime = new MoLangRuntime();
				//SetEnvironment(runtime.Environment);
				runtime.Environment.Structs.TryAdd("query", InitQueryStruct());

				foreach (var init in ConditionalExecute(runtime, definition.Scripts.Initialize))
				{
					
				}

				List<IExpression> preRender = new List<IExpression>();

				foreach (var list in definition.Scripts.PreAnimation)
				{
					preRender.AddRange(list);
				}

				Dictionary<string, Animation> animations = new Dictionary<string, Animation>();

				foreach (var kv in definition.Animations)
				{
					if (Alex.Instance.Resources.BedrockResourcePack.Animations.TryGetValue(kv.Value, out var animation))
					{
						animations.TryAdd(kv.Key, animation);
					}
				}

				_animations = animations;
				_entityDefinition = definition;
				_preRenderExpressions = preRender;
				Runtime = runtime;
			}
			finally
			{
				Monitor.Exit(_lock);
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
			q.Add("is_on_ground", mo => new DoubleValue(Entity.KnownPosition.OnGround ? 1d : 0d));
			q.Add("is_alive", mo => new DoubleValue(Entity.HealthManager.Health > 0 ? 1d : 0d));
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
				return new DoubleValue(Entity.Movement.BlocksPerTick);
			});
			
			q.Add("delta_time", mo =>
			{
				return new DoubleValue(_deltaTimeStopwatch.Elapsed.TotalSeconds);
			});
			
			return new QueryStruct(q);
		}

		private IMoValue NoOp(MoParams mo)
		{
			return new DoubleValue(0);
		}

		private Stopwatch _deltaTimeStopwatch = new Stopwatch();
		public void Update(GameTime gameTime)
		{
			DoImportant();
			_deltaTimeStopwatch.Restart();
		}

		private void DoImportant()
		{
			if (!Monitor.TryEnter(_lock, 0))
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
				
				if (_preRenderExpressions != null)
					runtime.Execute(_preRenderExpressions);

				//	TryAnimate(runtime, animations, "humanoid_base_pose", gameTime);
				TryAnimate(runtime, animations, "move.arms");
				TryAnimate(runtime, animations, "move.legs");
				TryAnimate(runtime, animations, "cape");
				//TryAnimate(runtime, animations, "move.arms", gameTime);
				//TryAnimate(runtime, animations, "animation.humanoid.move", gameTime);
			}
			finally
			{
				Monitor.Exit(_lock);
			}
		}
		
		private void TryAnimate(MoLangRuntime runtime, IReadOnlyDictionary<string, Animation> animations, string name)
		{
			if (animations.TryGetValue(name, out var animation))
			{
				if (animation?.Bones == null)
					return;
				
				foreach (var bone in animation.Bones)
				{
					if (bone.Value == null) continue;

					if (Entity?.ModelRenderer != null && Entity.ModelRenderer.GetBone(bone.Key, out var modelBone))
					{
						var value = bone.Value;
						
						var rotationOutput = ConditionalExecute(runtime, value.Rotation).ToArray();
						modelBone.Rotation = GetVector3(modelBone.Rotation, rotationOutput);

						var positionOutputs = ConditionalExecute(runtime, value.Position).ToArray();
						modelBone.Position = GetVector3(modelBone.Position, positionOutputs);
					}
				}
			}
		}

		private Vector3 GetVector3(Vector3 currentValue, IMoValue[] values)
		{
			float x = currentValue.X, y = currentValue.Y, z = currentValue.Z;
							
			if (values.Length == 3)
			{
				x = values[0].AsFloat();
				y = values[1].AsFloat();
				z = values[2].AsFloat();
			}else if (values.Length == 1)
			{
				x = y = z = values[0].AsFloat();
			}

			return new Vector3(x, y, z);
		}
		
		public void OnTick()
		{
			
		}
	}
}