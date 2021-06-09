using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Alex.Common.Graphics;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils;
using Alex.Common.World;
using Alex.Graphics;
using Alex.Graphics.Camera;
using Alex.Graphics.Textures;
using Alex.MoLang.Runtime;
using Alex.ResourcePackLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Particles;
using NLog;

namespace Alex.Particles
{
	/// <summary>
	///		Placeholder for particle system.
	/// </summary>
	public class ParticleManager : DrawableGameComponent, ITicked
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ParticleManager));
		private ConcurrentDictionary<string, ParticleEmitter> _particles =
			new ConcurrentDictionary<string, ParticleEmitter>(StringComparer.OrdinalIgnoreCase);

		private SpriteBatch _spriteBatch;
		private GraphicsDevice _graphics;

		public int ParticleCount { get; private set; }

		private ConcurrentDictionary<string, ManagedTexture2D> _sharedTextures =
			new ConcurrentDictionary<string, ManagedTexture2D>();
		private ResourceManager ResourceManager { get; }
		public ParticleManager(Game game, GraphicsDevice device, ResourceManager resourceManager) : base(game)
		{
			_spriteBatch = new SpriteBatch(device);
			_graphics = device;

			_camera = new Camera();
			Visible = false;
			ResourceManager = resourceManager;
		}

		public void Load(BedrockResourcePack resourcePack)
		{
			foreach (var particle in resourcePack.Particles)
			{
				if (particle.Value?.Description?.Identifier == null)
					continue;
				
				if (_particles.ContainsKey(particle.Value.Description.Identifier))
					continue;

				var texturePath = particle.Value.Description.BasicRenderParameters.Texture;

				ManagedTexture2D particleTexture = null;

				if (!_sharedTextures.TryGetValue(texturePath
					, out particleTexture))
				{
					switch (texturePath)
					{
						case "atlas.terrain":
							particleTexture = ResourceManager.BlockAtlas.GetAtlas();
							break;
						case "atlas.items":
							particleTexture = ResourceManager.ItemAtlas.GetAtlas();
							break;
						default:
							if (resourcePack.TryGetTexture(
								texturePath, out var img))
							{
								particleTexture = TextureUtils.BitmapToTexture2D(this, _graphics, img);
								//particleTexture = pooled;
							}
							break;
					}

					if (particleTexture != null)
						_sharedTextures.TryAdd(texturePath, particleTexture);
				}
				
				if (particleTexture != null)
				{
					ParticleEmitter p = new ParticleEmitter(particleTexture, particle.Value);
					particleTexture.Use(this);

					if (!TryRegister(particle.Value.Description.Identifier, p))
					{
						Log.Warn($"Could not add particle (duplicate): {particle.Key}");
						particleTexture.Release(this);
					}
				}
				else
				{
					Log.Warn($"Failed to add particle (missing texture): {particle.Key}");
				}
			}
		}

		public bool TryRegister(string identifier, ParticleEmitter emitter)
		{
			if (!_particles.TryAdd(identifier, emitter))
			{
				return false;
			}

			return true;
		}

		private ICamera _camera;
		public void Initialize(ICamera camera)
		{
			Visible = true;
			
			_camera = camera;
			foreach (var particle in _particles)
			{
				particle.Value.Reset();
			}
		}

		public void Hide()
		{
			Visible = false;
		}

		/// <inheritdoc />
		public override void Draw(GameTime gameTime)
		{
			if (!Enabled || !Visible)
				return;

			int count = 0;
			_spriteBatch.Begin(
				SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.DepthRead,
				RasterizerState.CullCounterClockwise);

			try
			{
				foreach (var particle in _particles.Values)
				{
					count += particle.Draw(gameTime, _spriteBatch, _camera);
				}
			}
			finally
			{
				_spriteBatch.End();
			}

			ParticleCount = count;
		}

		//private double _accumulator = 0d;
		/// <inheritdoc />
		public override void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;
			
			foreach (var particle in _particles.Values)
			{
				particle.Update(gameTime);
			}
			
			/*_accumulator += gameTime.ElapsedGameTime.TotalMilliseconds;

			while (_accumulator >= 50d)
			{
				OnTick();
				_accumulator -= 50d;
			}*/
		}

		/// <inheritdoc />
		public void OnTick()
		{
			foreach (var particle in _particles.Values)
			{
				particle.Tick();
			}
		}

		private bool TryConvert(ParticleType type, out string value, out ParticleDataMode dataMode)
		{
			dataMode = ParticleDataMode.None;
			switch (type)
			{
				case ParticleType.Bubble:
					value = "minecraft:basic_bubble_particle";
					return true;

				case ParticleType.Critical:
					value = "minecraft:critical_hit_emitter";
					dataMode = ParticleDataMode.Scale;
					return true;

				case ParticleType.BlockForceField:
					break;

				case ParticleType.Smoke:
					value = "minecraft:basic_smoke_particle";
					dataMode = ParticleDataMode.Scale;
					return true;

				case ParticleType.Explode:
					value = "minecraft:explosion_particle";
					return true;

				case ParticleType.WhiteSmoke:
					value = "minecraft:campfire_smoke_particle";

					return true;

				case ParticleType.Flame:
					value = "minecraft:basic_flame_particle";
					return true;

				case ParticleType.Lava:
					value = "minecraft:lava_particle";
;					return true;

				case ParticleType.LargeSmoke:
					value = "minecraft:water_evaporation_actor_emitter";
					return true;

				case ParticleType.Redstone:
					value = "minecraft:redstone_wire_dust_particle";
					return true;

				case ParticleType.RisingRedDust:
					value = "minecraft:redstone_wire_dust_particle";
					return true;

				case ParticleType.ItemBreak:
					dataMode = ParticleDataMode.Item;
					value = "minecraft:breaking_item_icon";
					return true;

				case ParticleType.SnowballPoof:
					break;

				case ParticleType.LargeExplode:
					value = "minecraft:large_explosion";
					return true;

				case ParticleType.HugeExplode:
					value = "minecraft:huge_explosion_emitter";
					return true;

				case ParticleType.MobFlame:
					value = "minecraft:mobflame_single";
					return true;

				case ParticleType.Heart:
					value = "minecraft:heart_particle";
;					return true;

				case ParticleType.Terrain:
					dataMode = ParticleDataMode.BlockRuntimeId;
					value = "minecraft:breaking_item_terrain";
					return true;

				case ParticleType.TownAura:
					break;

				case ParticleType.Portal:
					value = "minecraft:mob_portal";
					return true;

				case ParticleType.WaterSplash:
					value = "minecraft:water_splash_particle";
					return true;

				case ParticleType.WaterWake:
					value = "minecraft:water_wake_particle";
					return true;

				case ParticleType.DripWater:
					value = "minecraft:water_drip_particle";
					return true;

				case ParticleType.DripLava:
					value = "minecraft:lava_drip_particle";

					return true;

				case ParticleType.DripHoney:
					value = "minecraft:honey_drip_particle";
					return true;

				case ParticleType.Dust:
					value = "minecraft:redstone_wire_dust_particle";
					dataMode = ParticleDataMode.Color;
					return true;

				case ParticleType.MobSpell:
					value = "minecraft:mobspell_emitter";
					return true;

				case ParticleType.MobSpellAmbient: 
					value = "minecraft:mobspell_emitter";
					return true;

				case ParticleType.MobSpellInstantaneous:
					value = "minecraft:mobspell_emitter";
					return true;

				case ParticleType.Ink:
					value = "minecraft:ink_emitter";
					dataMode = ParticleDataMode.Scale;
					return true;

				case ParticleType.Slime:
					break;

				case ParticleType.RainSplash:
					value = "minecraft:rain_splash_particle";
					return true;

				case ParticleType.VillagerAngry:
					value = "minecraft:villager_angry";
					return true;

				case ParticleType.VillagerHappy:
					value = "minecraft:villager_happy";
					return true;

				case ParticleType.EnchantmentTable:
					value = "minecraft:enchanting_table_particle";
					return true;

				case ParticleType.TrackingEmitter:
					break;

				case ParticleType.Note:
					value = "minecraft:note_particle";
					return true;

				case ParticleType.WitchSpell:
					value = "minecraft:splash_spell_emitter";
					return true;

				case ParticleType.Carrot:
					break;

				case ParticleType.Unknown39:
					break;

				case ParticleType.EndRod:
					value = "minecraft:endrod";

					return true;

				case ParticleType.DragonsBreath:
					break;

				case ParticleType.Spit:
					value = "minecraft:llama_spit_smoke";
					return true;

				case ParticleType.Totem:
					value = "minecraft:totem_particle";
					return true;

				case ParticleType.Food:
					break;

				case ParticleType.FireworksStarter:
					value = "minecraft:sparkler_emitter";
					break;

				case ParticleType.FireworksSpark:
					value = "minecraft:sparkler_emitter";
					return true;

				case ParticleType.FireworksOverlay:
					value = "minecraft:sparkler_emitter";
					break;

				case ParticleType.BalloonGas:
					value = "minecraft:balloon_gas_particle";
					return true;

				case ParticleType.ColoredFlame:
					value = "minecraft:colored_flame_particle";

					return true;

				case ParticleType.Sparkler:
					value = "minecraft:sparkler_emitter";
					return true;

				case ParticleType.Conduit:
					value = "minecraft:conduit_particle";
					return true;

				case ParticleType.BubbleColumnUp:
					value = "minecraft:bubble_column_up_particle";
					return true;

				case ParticleType.BubbleColumnDown:
					value = "minecraft:bubble_column_down_particle";
					return true;

				case ParticleType.Sneeze:
					break;
			}
			value = null;
			return false;
		}

		public bool SpawnParticle(ParticleType type, Vector3 position, long data = 0)
		{
			if (!Enabled)
				return true;

			if (type == ParticleType.TrackingEmitter)
				return true;
			
			if (TryConvert(type, out var str, out ParticleDataMode dataMode))
			{
				return SpawnParticle(str, position, data, dataMode);
			}

			return false;
		}
		
		public bool SpawnParticle(string name, Vector3 position, long data = 0, ParticleDataMode dataMode = ParticleDataMode.None)
		{
			return SpawnParticle(name, position, out _, data, dataMode);
		}
		
		public bool SpawnParticle(string name, Vector3 position, out ParticleInstance instance, long data = 0, ParticleDataMode dataMode = ParticleDataMode.None)
		{
			instance = null;
			
			if (!Enabled)
				return true;
			
			if (_particles.TryGetValue(name, out var p))
			{
				return p.Spawn(position, data, dataMode, out instance);
				//return true;
			}

			return false;
		}

		/// <inheritdoc />
		protected override void UnloadContent()
		{
			base.UnloadContent();
			
		}
	}

	public enum ParticleDataMode
	{
		None,
		Color,
		Scale,
		BlockRuntimeId,
		Item
	}
}