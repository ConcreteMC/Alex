using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Alex.API.Graphics;
using Alex.API.Utils.Collections;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Bedrock.Particles;
using Alex.ResourcePackLib.Json.Bedrock.Particles.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Particles
{
	public class Particle
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Particle));
		private ThreadSafeList<ParticleInstance> _instances = new ThreadSafeList<ParticleInstance>();
		public PooledTexture2D Texture { get; }

		private AppearanceComponent AppearanceComponent { get; }
		public int MaxParticles { get; set; } = 500;
		private ParticleDefinition Definition { get; }
		public Particle(PooledTexture2D texture, ParticleDefinition definition)
		{
			Texture = texture;
			Definition = definition;

			MaxParticles = definition.MaxParticles;
			if (definition.Components.TryGetValue("minecraft:particle_appearance_billboard", out var value)
			    && value is AppearanceComponent apc)
			{
				AppearanceComponent = apc;
			}
			else
			{
				AppearanceComponent = null;
			}

		}

		public void Reset()
		{
			_instances.Clear();
		}

		public bool Spawn(Vector3 position, int data, ParticleDataMode dataMode, out ParticleInstance instance)
		{
			if (AppearanceComponent == null)
			{
				instance = null;
				return false;
			}

			MoLangRuntime runtime = new MoLangRuntime();

			instance = new ParticleInstance(this)
			{
				Position = position,
				//Lifetime = 10,
				Velocity = Definition.GetInitialSpeed(runtime),
				MaxLifetime = Definition.GetMaxLifetime(runtime),
				Acceleration = Definition.GetLinearAcceleration(runtime),
				DragCoEfficient = Definition.GetLinearDragCoEfficient(runtime),
				Runtime = runtime
			};

			runtime.Environment.Structs.TryAdd("query", instance);
			instance.SetData(data, dataMode);
			
			if (AppearanceComponent != null)
			{
				instance.UvPosition = AppearanceComponent.UV.GetUv(runtime);
				instance.UvSize = AppearanceComponent.UV.GetSize(runtime);
				
				var flipbook = AppearanceComponent.UV?.Flipbook;

				if (flipbook != null)
				{
					if (flipbook.MaxFrame != null)
					{
						instance.MaxFrame = runtime.Execute(flipbook.MaxFrame).AsFloat();
					}
				}
			}

			if (Definition.Components.TryGetValue("minecraft:particle_appearance_tinting", out var tinting)
			    && tinting is AppearanceTintingComponent atc)
			{
				if (dataMode != ParticleDataMode.Color && atc.Color != null)
				{
					instance.Color = atc.Color.GetValue(runtime);
				}
			}

			foreach (var component in Definition.Components)
			{
				component.Value?.OnCreate(runtime);
			}

			_instances.Add(instance);

			return true;
		}

		public void Tick()
		{
			if (_instances.Count == 0)
				return;

			List<ParticleInstance> toRemove = new List<ParticleInstance>();
			foreach (var instance in _instances)
			{
				if (instance.Lifetime >= instance.MaxLifetime)
				{
					toRemove.Add(instance);
				}
				else
				{
					foreach (var component in Definition.Components)
					{
						component.Value.Update(instance.Runtime);
						component.Value.PreRender(instance.Runtime);
					}
					
					if (AppearanceComponent != null)
					{ //instance.UvBasePosition = AppearanceComponent.UV.GetUv(instance.Runtime);
						instance.Size = AppearanceComponent.Size.Evaluate(instance.Runtime, instance.Size) * 16f;
						
						var flipbook = AppearanceComponent.UV?.Flipbook;

						if (flipbook != null && flipbook.FPS.HasValue)
						{
							var frame = (int) ((instance.Lifetime * flipbook.FPS.Value) % instance.MaxFrame);

							instance.UvPosition = AppearanceComponent.UV.GetUv(instance.Runtime) + flipbook.Step * frame;
						}
					}
					
					instance?.OnTick();
				}
			}

			foreach (var removed in toRemove)
				_instances.Remove(removed);
		}

		public void Update(GameTime gameTime)
		{
			foreach (var instance in _instances)
			{
				instance.Update(gameTime);
			}
		}

		public int Draw(GameTime gameTime, SpriteBatch spriteBatch, ICamera camera)
		{
			int count = 0;

			foreach (var instance in _instances)
			{
				if (count >= MaxParticles)
					continue;

				var pos = instance.Position;

				//var scale = 1f - (Vector3.DistanceSquared(camera.Position, pos) / camera.FarDistance);
				//if (scale <= 0f)
				//	continue;
				
				var screenSpace = spriteBatch.GraphicsDevice.Viewport.Project(
					pos, camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);
				
				bool isOnScreen = spriteBatch.GraphicsDevice.Viewport.Bounds.Contains((int) screenSpace.X, (int) screenSpace.Y);

				if (!isOnScreen) continue;
				
				count++;
				float depth = screenSpace.Z;
				float scale =  1f - (Vector3.Distance(camera.Position, pos) / camera.FarDistance);// 1.0f / depth;
				if (scale <= 0f)
					continue;

				if (scale > 1f)
					scale = 1f;
				
				Vector2 textPosition;
				textPosition.X = screenSpace.X;
				textPosition.Y = screenSpace.Y;

				var textureLocation = instance.UvPosition;
				var textureSize = instance.UvSize;

				spriteBatch.Draw(
					Texture, textPosition, new Rectangle(textureLocation.ToPoint(), textureSize.ToPoint()),
					instance.Color, 0f, Vector2.Zero,
					new Vector2( scale * instance.Scale * instance.Size.X, scale * instance.Scale * instance.Size.Y),
					SpriteEffects.None, depth);
			}

			return count;
		}
	}

	public class ParticleInstance : QueryStruct
	{
		private Particle _parent;
		public ParticleInstance(Particle parent)
		{
			_parent = parent;
			
			Functions.Add("frame_alpha", mo => _deltaTime.TotalMilliseconds);
			Functions.Add("spellcolor", mo => new QueryStruct(new []
			{
				new KeyValuePair<string, Func<MoParams, object>>("r", mo2 => (int)Color.R),
				new KeyValuePair<string, Func<MoParams, object>>("g", mo2 => (int)Color.G),
				new KeyValuePair<string, Func<MoParams, object>>("b", mo2 => (int)Color.B),
				new KeyValuePair<string, Func<MoParams, object>>("a", mo2 => (int)Color.A)
			}));
		}

		public double Lifetime { get; set; } = 0d;
		
		public Vector3 Velocity { get; set; } = Vector3.Zero;
		public Vector3 Position { get; set; } = Vector3.Zero;
		public Vector3 Acceleration { get; set; } = Vector3.Zero;
		public float DragCoEfficient { get; set; } = 0f;
		public double MaxLifetime { get; set; } = 0.5D;
		public float MaxFrame { get; set; } = 1f;
		
		public Vector2 UvPosition { get; set; } = Vector2.Zero;
		public Vector2 UvSize { get; set; } = Vector2.One;
		public Vector2 Size { get; set; } = Vector2.One;
		public float Scale { get; set; } = 1f;
		public Color Color { get; set; } = Color.White;
		public MoLangRuntime Runtime { get; set; }
		
		private TimeSpan _deltaTime = TimeSpan.Zero;

		public void Update(GameTime gameTime)
		{
			_deltaTime = gameTime.ElapsedGameTime;

			var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			Lifetime += dt;

			Position += Velocity * dt;
			Velocity += Acceleration * dt;
			Acceleration = -DragCoEfficient * Velocity;
		}

		public void OnTick()
		{
			var variableStruct = Runtime.Environment.Structs["variable"];
					
			variableStruct.Set("particle_age", new DoubleValue(Lifetime));
			variableStruct.Set("particle_lifetime", new DoubleValue(MaxLifetime));
			
			
		}
		
		public void SetData(int data, ParticleDataMode dataMode)
		{
			if (data == 0) return;
			switch (dataMode)
			{
				case ParticleDataMode.Color:
				{
					var a = (byte)((data >> 24)  & 0xFF);
					var r = (byte)((data >> 16)  & 0xFF);
					var g = (byte)((data >> 8)   & 0xFF);
					var b = (byte)(data & 0xFF);
					Color = new Color(r, g, b, a);
				} break;

				case ParticleDataMode.Scale:
				{
					Scale = data;
				} break;
			}
		}
	}
}