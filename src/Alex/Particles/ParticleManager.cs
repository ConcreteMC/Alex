using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.API.World;
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
	public class ParticleManager : ITicked
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ParticleManager));
		private ConcurrentDictionary<string, Particle> _particles =
			new ConcurrentDictionary<string, Particle>(StringComparer.OrdinalIgnoreCase);

		private SpriteBatch _spriteBatch;
		private GraphicsDevice _graphics;

		public bool Enabled { get; set; } = true;
		public ParticleManager(GraphicsDevice device) : base()
		{
			_spriteBatch = new SpriteBatch(device);
			_graphics = device;
		}

		public void Load(BedrockResourcePack resourcePack)
		{
			foreach (var particle in resourcePack.Particles)
			{
				if (particle.Value?.Description?.Identifier == null)
					continue;
				
				if (_particles.ContainsKey(particle.Value.Description.Identifier))
					continue;

				if (resourcePack.TryGetTexture(
					particle.Value.Description.BasicRenderParameters.Texture, out var texture))
				{
					var pooled = TextureUtils.BitmapToTexture2D(this, _graphics, texture);

					Particle p = new Particle(pooled, particle.Value);

					if (particle.Key == "minecraft:falling_dust" || particle.Key == "minecraft:redstone_wire_dust_particle")
						p.HasColor = true;
					
					if (!_particles.TryAdd(particle.Value.Description.Identifier, p))
					{
						Log.Info($"Could not add particle: {particle.Key}");
					}
				}
				else
				{
					Log.Warn($"Failed to add particle: {particle.Key}");
				}
			}
		}

		public void Reset()
		{
			foreach (var particle in _particles)
			{
				particle.Value.Reset();
			}
		}

		/// <inheritdoc />
		public void Draw(GameTime gameTime, ICamera camera)
		{
			if (!Enabled)
				return;
			
			_spriteBatch.Begin(
				SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.DepthRead,
				RasterizerState.CullNone);

			try
			{
				foreach (var particle in _particles)
				{
					particle.Value.Draw(gameTime, _spriteBatch, camera);
				}
			}
			finally
			{
				_spriteBatch.End();
			}
		}

		//private double _accumulator = 0d;
		/// <inheritdoc />
		public void Update(GameTime gameTime)
		{
			if (!Enabled)
				return;
			
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
			foreach (var particle in _particles)
			{
				particle.Value.Tick();
			}
		}

		private bool TryConvert(ParticleType type, out string value)
		{
			switch (type)
			{
				case ParticleType.Bubble:
					value = "minecraft:basic_bubble_particle";
					return true;

				case ParticleType.Critical:
					value = "minecraft:critical_hit_emitter";
					return true;

				case ParticleType.BlockForceField:
					break;

				case ParticleType.Smoke:
					value = "minecraft:basic_smoke_particle";
					return true;

				case ParticleType.Explode:
					value = "minecraft:explode";
					return true;

				case ParticleType.WhiteSmoke:
					break;

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
					break;

				case ParticleType.ItemBreak:
					break;

				case ParticleType.SnowballPoof:
					break;

				case ParticleType.LargeExplode:
					break;

				case ParticleType.HugeExplode:
					break;

				case ParticleType.MobFlame:
					value = "minecraft:mobflame_single";
					return true;

				case ParticleType.Heart:
					value = "minecraft:heart_particle";
;					return true;

				case ParticleType.Terrain:
					break;

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
					break;

				case ParticleType.DripWater:
					value = "minecraft:water_drip_particle";
					return true;

				case ParticleType.DripLava:
					value = "minecraft:lava_drip_particle";

					return true;

				case ParticleType.DripHoney:
					break;

				case ParticleType.Dust:
					value = "minecraft:redstone_wire_dust_particle";
					return true;

				case ParticleType.MobSpell:
					value = "minecraft:mobspell_emitter";
					return true;

				case ParticleType.MobSpellAmbient:
					break;

				case ParticleType.MobSpellInstantaneous:
					break;

				case ParticleType.Ink:
					value = "minecraft:ink_emitter";
					break;

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
					break;

				case ParticleType.Carrot:
					break;

				case ParticleType.Unknown39:
					break;

				case ParticleType.EndRod:
					break;

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
					break;

				case ParticleType.FireworksSpark:
					break;

				case ParticleType.FireworksOverlay:
					break;

				case ParticleType.BalloonGas:
					break;

				case ParticleType.ColoredFlame:
					break;

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

		public bool SpawnParticle(ParticleType type, Vector3 position, int data = 0)
		{
			if (!Enabled)
				return true;
			
			if (TryConvert(type, out var str))
			{
				return SpawnParticle(str, position, data);
			}

			return false;
		}
		
		public bool SpawnParticle(string name, Vector3 position, int data = 0)
		{
			if (!Enabled)
				return true;
			
			if (_particles.TryGetValue(name, out var p))
			{
				p.Spawn(position, data);
				return true;
			}

			return false;
		}
	}
}