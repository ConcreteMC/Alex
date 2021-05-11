using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Utils;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Items;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.ResourcePackLib.Json.Bedrock.Particles;
using Alex.ResourcePackLib.Json.Bedrock.Particles.Components;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Particles
{
	public class ParticleInstance : QueryStruct, IParticle
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ParticleInstance));
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
			
			variables.Set("emitter_texture_coordinate.u", new DoubleValue(0));
			variables.Set("emitter_texture_coordinate.v", new DoubleValue(0));
			
			//emitter_texture_size
			variables.Set("emitter_texture_size.u", new DoubleValue(0));
			variables.Set("emitter_texture_size.v", new DoubleValue(0));
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
		public Color Color
		{
			get => _color;
			set
			{
				_color = value;
				SetVariable("color.r", new DoubleValue(value.R));
				SetVariable("color.g", new DoubleValue(value.G));
				SetVariable("color.b", new DoubleValue(value.B));
				SetVariable("color.a", new DoubleValue(value.A));
			}
		}

		public MoLangRuntime Runtime { get; }
		
		private TimeSpan _deltaTime = TimeSpan.Zero;
		private Color _color = Color.White;

		public void Update(GameTime gameTime)
		{
			_deltaTime = gameTime.ElapsedGameTime;

			var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			Lifetime += dt;

			Position += Velocity * dt;
			Velocity += Acceleration * dt;
			Acceleration = -DragCoEfficient * Velocity;
		}

		private void SetVariable(string key, IMoValue value)
		{
			Runtime.Environment.Structs["variable"].Set(key, value);
		}

		public void OnTick()
		{
			var variableStruct = Runtime.Environment.Structs["variable"];
					
			//variableStruct.Set("particle_random_1", new DoubleValue(MaxLifetime * 10));
			variableStruct.Set("particle_age", new DoubleValue(Lifetime));
			variableStruct.Set("particle_lifetime", new DoubleValue(MaxLifetime));
			
			
		}
		
		public void SetData(long data, ParticleDataMode dataMode)
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

				case ParticleDataMode.Item:
				{
					if (ItemFactory.ResolveItemName((int) data, out string name))
					{
						if (ItemFactory.TryGetItem(name, out Item item))
						{
							var firstTexture = item.Renderer.Model.Textures.FirstOrDefault();

							if (firstTexture.Value == null)
							{
								Log.Warn($"Invalid item, Reason=no textures, id={(int) data}, name={name}");
								return;
							}
					
							var atlasLocation = Alex.Instance.Resources.ItemAtlas.GetAtlasLocation(firstTexture.Value);
							SetVariable("emitter_texture_coordinate.u", new DoubleValue(atlasLocation.Position.X));
							SetVariable("emitter_texture_coordinate.v", new DoubleValue(atlasLocation.Position.Y));
			
							//emitter_texture_size
							SetVariable("emitter_texture_size.u", new DoubleValue(atlasLocation.Width));
							SetVariable("emitter_texture_size.v", new DoubleValue(atlasLocation.Height));
						}
						else
						{
							Log.Warn($"Invalid item. Reason=No item found with name: {name}");
						}
					}
					else
					{
						Log.Warn($"Invalid item, could not resolve to name. ID={data}");
					}

				//	BlockFactory.StateIDToRaw((uint) data, out int id, out byte meta);
				//	if (!ItemFactory.TryGetItem((short) id, (short) meta, out var item))
				//	{
				//		Log.Warn($"Invalid item, id={id}, meta={meta}");

				//		return;
				//	}
				
				} break;

				case ParticleDataMode.BlockRuntimeId:
				{
					var bs = BlockFactory.GetBlockState((uint) data);

					if (bs == null)
					{
						Log.Warn($"Blockstate id invalid: {data}");

						return;
					}

					var model = bs.ModelData.FirstOrDefault();
					if (model?.ModelName == null)
					{
						Log.Warn($"Blockstate invalid, modelname was null: {bs.ToString()}");

						return;
					}
					
					//string texture = nu
					if (Alex.Instance.Resources.BlockModelRegistry.TryGet(model.ModelName, out var registryEntry))
					{
						var texture = registryEntry.Value.Textures.FirstOrDefault().Value;

						if (texture == null)
						{
							Log.Warn($"Blockstate invalid, no textures in model was null: {bs.ToString()}");

							return;
						}
						
						var atlasLocation = Alex.Instance.Resources.BlockAtlas.GetAtlasLocation(texture);
						SetVariable("emitter_texture_coordinate.u", new DoubleValue(atlasLocation.Position.X));
						SetVariable("emitter_texture_coordinate.v", new DoubleValue(atlasLocation.Position.Y));
			
						//emitter_texture_size
						SetVariable("emitter_texture_size.u", new DoubleValue(atlasLocation.Width));
						SetVariable("emitter_texture_size.v", new DoubleValue(atlasLocation.Height));
					}
				//	if (bs.ModelData[0]. == null)
				//	{
				//		Log.Warn($"Got invalid runtime blockstate: {bs.ToString()}");
				//		return;
				//	}
				
					
				} break;
			}
		}
	}
}