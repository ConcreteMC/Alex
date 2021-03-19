using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Alex.API.Graphics;
using Alex.API.Utils.Collections;
using Alex.MoLang.Runtime;
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
		public int MaxParticles { get; set; } = 50;
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

		public bool Spawn(Vector3 position, int data)
		{
			if (AppearanceComponent == null)
			{
				return false;
			}

			MoLangRuntime runtime = new MoLangRuntime();

			ParticleInstance instance = new ParticleInstance(this)
			{
				Position = position,
				//Lifetime = 10,
				Data = data,
				Velocity = Definition.GetInitialSpeed(runtime),
				MaxLifetime = Definition.GetMaxLifetime(runtime),
				Acceleration = Definition.GetLinearAcceleration(runtime),
				DragCoEfficient = Definition.GetLinearDragCoEfficient(runtime),
				Runtime = runtime
			};
			
			if (AppearanceComponent != null)
			{
				instance.UvBasePosition = AppearanceComponent.UV.GetUv(runtime);
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
					instance.Runtime.Environment.SetValue("variable.particle_age", new DoubleValue(instance.Lifetime));

					if (AppearanceComponent != null)
					{ //instance.UvBasePosition = AppearanceComponent.UV.GetUv(instance.Runtime);
						instance.Size = AppearanceComponent.Size.Evaluate(instance.Runtime, instance.Size) * 10f;
					}
				}
			}

			foreach (var removed in toRemove)
				_instances.Remove(removed);
		}

		public int Draw(GameTime gameTime, SpriteBatch spriteBatch, ICamera camera)
		{
			float dt = (float) gameTime.ElapsedGameTime.TotalSeconds;

			int count = 0;

			foreach (var instance in _instances)
			{
				instance.Lifetime += dt;
				//instance.Acceleration = -instance.DragCoEfficient * instance.Velocity;

				instance.Position += instance.Velocity * dt;
				instance.Velocity += instance.Acceleration * dt;

				if (count >= MaxParticles)
					continue;

				count++;

				var pos = instance.Position;

				Vector2 textPosition;

				var screenSpace = spriteBatch.GraphicsDevice.Viewport.Project(
					pos, camera.ProjectionMatrix, camera.ViewMatrix, Matrix.Identity);

				textPosition.X = screenSpace.X;
				textPosition.Y = screenSpace.Y;

				var textureLocation = instance.UvBasePosition;
				var textureSize = instance.UvSize;

				if (AppearanceComponent != null)
				{
					var flipbook = AppearanceComponent.UV?.Flipbook;

					if (flipbook != null && flipbook.FPS.HasValue)
					{
						var frame = (int) ((instance.Lifetime * flipbook.FPS.Value) % instance.MaxFrame);

						textureLocation -= flipbook.Step * frame;
					}
				}

				spriteBatch.Draw(
					Texture, textPosition, new Rectangle(textureLocation.ToPoint(), textureSize.ToPoint()),
					instance.Color, 0f, Vector2.Zero,
					new Vector2((16f / textureSize.X) * instance.Size.X, (16f / textureSize.Y) * instance.Size.Y),
					SpriteEffects.None, screenSpace.Z);
			}

			return count;
		}

		public bool HasColor { get; set; } = false;
	}

	public class ParticleInstance
	{
		private Particle _parent;
		public ParticleInstance(Particle parent)
		{
			_parent = parent;
		}

		public double Lifetime { get; set; } = 0d;
		
		private int _data = 0;
		public Vector3 Velocity { get; set; } = Vector3.Zero;
		public Vector3 Position { get; set; } = Vector3.Zero;
		public Vector3 Acceleration { get; set; } = Vector3.Zero;
		public float DragCoEfficient { get; set; } = 0f;
		public double MaxLifetime { get; set; } = 0.5D;
		public float MaxFrame { get; set; } = 8f;
		
		public Vector2 UvBasePosition { get; set; } = Vector2.Zero;
		public Vector2 UvSize { get; set; } = Vector2.One;
		public Vector2 Size { get; set; } = Vector2.One;
		public int Data
		{
			get => _data;
			set
			{
				_data = value;

				if (_parent.HasColor)
				{
					var a = (byte)((value >> 24)  & 0xF);
					var r = (byte)((value >> 16)  & 0xF);
					var g = (byte)((value >> 8)   & 0xF);
					var b = (byte)(value & 0xF);
					Color = new Color(r, g, b, a);
				}	
			}
		}

		public Color Color { get; set; } = Color.White;
		public MoLangRuntime Runtime { get; set; }
	}
}