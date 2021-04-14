using System;
using System.Collections.Generic;
using Alex.API.Utils;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Bedrock.Particles;
using Alex.ResourcePackLib.Json.Bedrock.Particles.Components;
using Microsoft.Xna.Framework;

namespace Alex.Particles
{
	public class ParticleInstance : QueryStruct, IParticle
	{
		private ParticleEmitter _parent;
		public ParticleInstance(ParticleEmitter parent, MoLangRuntime runtime, Vector3 position)
		{
			_parent = parent;
			Runtime = runtime;
			Position = position;

			if (parent.Definition.Components.TryGetValue("minecraft:particle_appearance_tinting", out var tinting)
			    && tinting is AppearanceTintingComponent atc)
			{
				if (atc.Color != null)
					Color = atc.Color.GetValue(runtime);
			}
			
			Functions.Add("frame_alpha", mo => _deltaTime.TotalSeconds);
			Functions.Add("spellcolor", mo => new VariableStruct(new []
			{
				new KeyValuePair<string, IMoValue>("r", new DoubleValue((int)Color.R)),
				new KeyValuePair<string, IMoValue>("g", new DoubleValue((int)Color.G)),
				new KeyValuePair<string, IMoValue>("b",new DoubleValue((int)Color.B)),
				new KeyValuePair<string, IMoValue>("a", new DoubleValue((int)Color.A))
			}));
			
			var variables = runtime.Environment.Structs["variable"];
			variables.Set("particle_random_1", new DoubleValue(FastRandom.Instance.NextDouble()));
			variables.Set("particle_random_2", new DoubleValue(FastRandom.Instance.NextDouble()));
			variables.Set("particle_random_3", new DoubleValue(FastRandom.Instance.NextDouble()));
			variables.Set("particle_random_4", new DoubleValue(FastRandom.Instance.NextDouble()));
		}

		/// <summary>
		///		The velocity of the particle
		/// </summary>
		public Vector3 Velocity { get; set; } = Vector3.Zero;
		
		/// <summary>
		///		The position of the particle
		/// </summary>
		public Vector3 Position { get; set; } = Vector3.Zero;
		
		/// <summary>
		///		The acceleration applied to the particle
		/// </summary>
		public Vector3 Acceleration { get; set; } = Vector3.Zero;
		
		/// <summary>
		///		The drag co-efficient applied to the particle
		/// </summary>
		public float DragCoEfficient { get; set; } = 0f;
		
		/// <summary>
		///		How long this particle has been visible for
		/// </summary>
		public double Lifetime { get; set; } = 0d;
		
		/// <summary>
		///		How long this particle can be visible for
		/// </summary>
		public double MaxLifetime { get; set; } = 0.5D;
		
		/// <summary>
		///		The total amount of frames for this particle.
		/// </summary>
		public float FrameCount { get; set; } = 1f;
		
		/// <summary>
		///		The position of the sprite on the spritesheet
		/// </summary>
		public Vector2 UvPosition { get; set; } = Vector2.Zero;
		
		/// <summary>
		///		The size of the sprite on the spritesheet
		/// </summary>
		public Vector2 UvSize { get; set; } = Vector2.One;
		
		/// <summary>
		///		Specifies the x and y size of the billboard.
		/// </summary>
		public Vector2 Size { get; set; } = Vector2.One;
		
		/// <summary>
		///		The color of the particle
		/// </summary>
		public Color Color { get; set; } = Color.White;
		public MoLangRuntime Runtime { get; }
		
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
					
			//variableStruct.Set("particle_random_1", new DoubleValue(MaxLifetime * 10));
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
					//Scale = data;
				} break;
			}
		}
	}
}