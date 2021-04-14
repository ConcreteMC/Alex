using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.Utils.Collections;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Bedrock.Particles;
using Alex.ResourcePackLib.Json.Bedrock.Particles.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Particles
{
	public class ParticleEmitter
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ParticleEmitter));
		private ThreadSafeList<ParticleInstance> _instances = new ThreadSafeList<ParticleInstance>();
		public PooledTexture2D Texture { get; }

		private AppearanceComponent AppearanceComponent { get; }
		public int MaxParticles { get; set; } = 500;
		public ParticleDefinition Definition { get; }
		public ParticleEmitter(PooledTexture2D texture, ParticleDefinition definition)
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

			instance = new ParticleInstance(this, runtime, position);

			runtime.Environment.Structs.TryAdd("query", instance);
			instance.SetData(data, dataMode);

			foreach (var component in Definition.Components)
			{
				component.Value?.OnCreate(instance, runtime);
			}
			
			_instances.Add(instance);

			return true;
		}

		public void Tick()
		{
			if (_instances.Count == 0)
				return;

			var rnd1 = FastRandom.Instance.NextDouble();
			var rnd2 = FastRandom.Instance.NextDouble();
			var rnd3 = FastRandom.Instance.NextDouble();
			var rnd4 = FastRandom.Instance.NextDouble();

			List<ParticleInstance> toRemove = new List<ParticleInstance>();

			foreach (var instance in _instances)
			{
				if (instance.Lifetime >= instance.MaxLifetime)
				{
					toRemove.Add(instance);
				}
				else
				{
					var variables = instance.Runtime.Environment.Structs["variable"];
					variables.Set("emitter_random_1", new DoubleValue(rnd1));
					variables.Set("emitter_random_2", new DoubleValue(rnd2));
					variables.Set("emitter_random_3", new DoubleValue(rnd3));
					variables.Set("emitter_random_4", new DoubleValue(rnd4));

					foreach (var component in Definition.Components)
					{
						component.Value.Update(instance, instance.Runtime);
						component.Value.PreRender(instance, instance.Runtime);
					}

					instance.OnTick();
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
					//2f * ((scale)),
					new Vector2( scale * instance.Size.X, scale * instance.Size.Y) * 16f,
					SpriteEffects.None, depth);
			}

			return count;
		}
	}
}