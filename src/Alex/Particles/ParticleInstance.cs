using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Common.Graphics;
using Alex.Common.Utils;
using Alex.Items;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;
using Alex.ResourcePackLib.Json.Bedrock.Particles;
using Alex.ResourcePackLib.Json.Bedrock.Particles.Components;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Particles
{
	public class UvStruct : IMoStruct
	{
		private DoubleValue _u;
		private DoubleValue _v;

		/// <inheritdoc />
		public object Value => $"{U}, {V}";

		public double U
		{
			get => _u.Value;
			set
			{
				_u.Value = value;
			}
		}

		public double V
		{
			get => _v.Value;
			set
			{
				_v.Value = value;
			}
		}

		public UvStruct(Vector2 uv) : this(uv.X, uv.Y) { }

		public UvStruct(double u, double v)
		{
			_u = new DoubleValue(u);
			_v = new DoubleValue(v);
		}

		/// <inheritdoc />
		public void Set(MoPath key, IMoValue value)
		{
			if (key.HasChildren)
				throw new InvalidOperationException();

			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public IMoValue Get(MoPath key, MoParams parameters)
		{
			if (key.HasChildren)
				throw new InvalidOperationException();

			if (key.Value.Equals("u", StringComparison.OrdinalIgnoreCase))
			{
				return _u;
			}
			else if (key.Value.Equals("v", StringComparison.OrdinalIgnoreCase))
			{
				return _v;
			}

			return DoubleValue.Zero;
		}

		/// <inheritdoc />
		public void Clear()
		{
			throw new NotImplementedException();
		}
	}

	public class ColorStruct : IMoStruct
	{
		private DoubleValue _a;
		private DoubleValue _r;
		private DoubleValue _g;
		private DoubleValue _b;

		/// <inheritdoc />
		public object Value => $"{R}, {G}, {B}, {A}";

		public byte A
		{
			get => (byte)_a.Value;
			set
			{
				_a.Value = value;
			}
		}

		public byte R
		{
			get => (byte)_r.Value;
			set
			{
				_r.Value = value;
			}
		}

		public byte G
		{
			get => (byte)_g.Value;
			set
			{
				_g.Value = value;
			}
		}

		public byte B
		{
			get => (byte)_b.Value;
			set
			{
				_b.Value = value;
			}
		}

		public ColorStruct(Color color) : this(color.R, color.G, color.B, color.A) { }

		public ColorStruct(byte r, byte g, byte b, byte a)
		{
			_r = new DoubleValue(r);
			_g = new DoubleValue(g);
			_b = new DoubleValue(b);
			_a = new DoubleValue(a);
		}

		/// <inheritdoc />
		public void Set(MoPath key, IMoValue value)
		{
			if (key.HasChildren)
				throw new InvalidOperationException();

			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public IMoValue Get(MoPath key, MoParams parameters)
		{
			if (key.HasChildren)
				throw new InvalidOperationException();

			switch (key.Value.ToLowerInvariant())
			{
				case "a":
					return _a;

				case "r":
					return _r;

				case "g":
					return _g;

				case "b":
					return _b;
			}

			return DoubleValue.Zero;
		}

		/// <inheritdoc />
		public void Clear()
		{
			throw new NotImplementedException();
		}

		public static implicit operator Color(ColorStruct a)
		{
			return new Color(a.R, a.G, a.B, a.A);
		}
	}

	public class ParticleInstance : QueryStruct, IParticle
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ParticleInstance));

		public ParticleInstance(Vector3 position)
		{
			Runtime = new MoLangRuntime();
			Runtime.Environment.Structs.TryAdd("query", this);

			Position = position;
			_color = new ColorStruct(Color.White);

			Functions.Add("frame_alpha", mo => _deltaTime.TotalSeconds);
			Functions.Add("spellcolor", mo => _color);

			var variables = Runtime.Environment.Structs["variable"];
			variables.Set(new MoPath("particle_random_1"), new DoubleValue(FastRandom.Instance.NextDouble()));
			variables.Set(new MoPath("particle_random_2"), new DoubleValue(FastRandom.Instance.NextDouble()));
			variables.Set(new MoPath("particle_random_3"), new DoubleValue(FastRandom.Instance.NextDouble()));
			variables.Set(new MoPath("particle_random_4"), new DoubleValue(FastRandom.Instance.NextDouble()));

			variables.Set(new MoPath("emitter_texture_coordinate"), _emitterTextureCoordinate = new UvStruct(0, 0));

			//variables.Set(new MoPath("emitter_texture_coordinate.u"), new DoubleValue(0));
			//variables.Set(new MoPath("emitter_texture_coordinate.v"), new DoubleValue(0));

			//emitter_texture_size
			variables.Set(new MoPath("emitter_texture_size"), _emitterTextureSize = new UvStruct(0, 0));
			//variables.Set(new MoPath("emitter_texture_size.u"), new DoubleValue(0));
			//variables.Set(new MoPath("emitter_texture_size.v"), new DoubleValue(0));

			SetVariable("particle_age", _lifeTime = new DoubleValue(0d));
			SetVariable("particle_lifetime", _maxLifetime = new DoubleValue(0.5d));
		}

		private UvStruct _emitterTextureCoordinate;
		private UvStruct _emitterTextureSize;
		private ColorStruct _color;

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

		private DoubleValue _lifeTime;

		/// <summary>
		///		How long this particle has been visible for
		/// </summary>
		public double Lifetime
		{
			get
			{
				return _lifeTime.Value;
			}
			set
			{
				_lifeTime.Value = value;
			}
		}

		private DoubleValue _maxLifetime;

		/// <summary>
		///		How long this particle can be visible for
		/// </summary>
		public double MaxLifetime
		{
			get => _maxLifetime.Value;
			set
			{
				_maxLifetime.Value = value;
			}
		}

		/// <summary>
		///		The total amount of frames for this particle.
		/// </summary>
		public float FrameCount { get; set; } = 1f;

		/// <summary>
		///		The position of the sprite on the spritesheet
		/// </summary>
		public Vector2 UvPosition
		{
			get => _uvPosition;
			set
			{
				_uvPosition = value;
				UpdateRectangle();
			}
		}

		/// <summary>
		///		The size of the sprite on the spritesheet
		/// </summary>
		public Vector2 UvSize
		{
			get => _uvSize;
			set
			{
				_uvSize = value;
				UpdateRectangle();
			}
		}

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
				_color.R = value.R;
				_color.G = value.G;
				_color.B = value.B;
				_color.A = value.A;
			}
		}

		public Rectangle Rectangle { get; private set; }

		public MoLangRuntime Runtime { get; }

		private TimeSpan _deltaTime = TimeSpan.Zero;
		private Vector2 _uvPosition = Vector2.Zero;
		private Vector2 _uvSize = Vector2.One;

		public bool Valid { get; set; } = true;

		private void UpdateRectangle()
		{
			Rectangle = new Rectangle(_uvPosition.ToPoint(), _uvSize.ToPoint());
		}

		public void Update(GameTime gameTime)
		{
			_deltaTime = Alex.DeltaTimeSpan;

			var dt = Alex.DeltaTime;
			Lifetime += dt;

			Position += Velocity * dt;
			Velocity += Acceleration * dt;
			Acceleration = -DragCoEfficient * Velocity;
		}

		private void SetVariable(string key, IMoValue value)
		{
			Runtime.Environment.Structs["variable"].Set(new MoPath(key), value);
		}

		public float RenderScale { get; set; } = 1f;

		public void OnTick(ICamera camera)
		{
			RenderScale = 1f - (Vector3.Distance(camera.Position, Position) / camera.FarDistance);
		}

		public void SetData(long data, ParticleDataMode dataMode)
		{
			if (data == 0) return;

			switch (dataMode)
			{
				case ParticleDataMode.Color:
				{
					var a = (byte)((data >> 24) & 0xFF);
					var r = (byte)((data >> 16) & 0xFF);
					var g = (byte)((data >> 8) & 0xFF);
					var b = (byte)(data & 0xFF);
					Color = new Color(r, g, b, a);
				}

					break;

				case ParticleDataMode.Scale:
				{
					//Scale = data;
				}

					break;

				case ParticleDataMode.Item:
				{
					if (ItemFactory.ResolveItemName((int)data, out var name))
					{
						if (ItemFactory.TryGetItem(name, out Item item))
						{
							var firstTexture = item.Renderer.ResourcePackModel.Textures.FirstOrDefault();

							if (firstTexture.Value == null)
							{
								Log.Warn($"Invalid item, Reason=no textures, id={(int)data}, name={name}");

								return;
							}

							var atlasLocation = Alex.Instance.Resources.ItemAtlas.GetAtlasLocation(firstTexture.Value);
							_emitterTextureCoordinate.U = atlasLocation.Position.X;
							_emitterTextureCoordinate.V = atlasLocation.Position.Y;
							//SetVariable("emitter_texture_coordinate.u", new DoubleValue(atlasLocation.Position.X));
							//SetVariable("emitter_texture_coordinate.v", new DoubleValue(atlasLocation.Position.Y));

							//emitter_texture_size
							_emitterTextureSize.U = atlasLocation.Width;
							_emitterTextureSize.V = atlasLocation.Height;
							//SetVariable("emitter_texture_size.u", new DoubleValue(atlasLocation.Width));
							//SetVariable("emitter_texture_size.v", new DoubleValue(atlasLocation.Height));
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
				}

					break;

				case ParticleDataMode.BlockRuntimeId:
				{
					var bs = BlockFactory.GetBlockState((uint)data);

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
						_emitterTextureCoordinate.U = atlasLocation.Position.X;
						_emitterTextureCoordinate.V = atlasLocation.Position.Y;
						//SetVariable("emitter_texture_coordinate.u", new DoubleValue(atlasLocation.Position.X));
						//SetVariable("emitter_texture_coordinate.v", new DoubleValue(atlasLocation.Position.Y));

						//emitter_texture_size
						_emitterTextureSize.U = atlasLocation.Width;
						_emitterTextureSize.V = atlasLocation.Height;
						//SetVariable("emitter_texture_size.u", new DoubleValue(atlasLocation.Width));
						//SetVariable("emitter_texture_size.v", new DoubleValue(atlasLocation.Height));
					}
					//	if (bs.ModelData[0]. == null)
					//	{
					//		Log.Warn($"Got invalid runtime blockstate: {bs.ToString()}");
					//		return;
					//	}
				}

					break;
			}
		}
	}
}