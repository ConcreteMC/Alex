using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.Gamestates;
using Alex.Graphics.Models;
using Alex.Rendering.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Entities;
using MiNET.Entities.Hostile;
using MiNET.Entities.Passive;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;

namespace Alex.Utils
{
    public static class MiNETUtils
    {
	    public static Vector3 ToXnaVector3(this PlayerLocation location)
	    {
			return new Vector3(location.X, location.Y, location.Z);
	    }

	    public static Vector3 ToXnaVector3(this System.Numerics.Vector3 location)
	    {
		    return new Vector3(location.X, location.Y, location.Z);
	    }
	}

	public static class EntityExtensions
	{
		private static ConcurrentDictionary<long, EntityModelRenderer> _entityRenderers = new ConcurrentDictionary<long, EntityModelRenderer>();
		private static ConcurrentDictionary<long, UUID> _entityUuids = new ConcurrentDictionary<long, UUID>();
		public static EntityModelRenderer GetModelRenderer(this Entity entity)
		{
			if (_entityRenderers.TryGetValue(entity.EntityId, out EntityModelRenderer val))
			{
				return val;
			}

			return null;
		}

		public static void SetModelRenderer(this Entity entity, EntityModelRenderer value)
		{
			_entityRenderers.AddOrUpdate(entity.EntityId, value, (a, b) => value);
		}

		public static UUID GetUUID(this Entity entity)
		{
			if (_entityUuids.TryGetValue(entity.EntityId, out UUID v))
			{
				return v;
			}

			return null;
		}

		public static void SetUUID(this Entity entity, UUID value)
		{
			_entityUuids.AddOrUpdate(entity.EntityId, value, (a, b) => value);
		}

		internal static void DeleteData(this Entity entity)
		{
			if (entity != null)
				_entityRenderers.TryRemove(entity.EntityId, out var _);

			if (entity != null)
				_entityUuids.TryRemove(entity.EntityId, out _);
		}

		public static void RenderNametag(this Entity entity, IRenderArgs renderArgs, Camera camera)
		{
			Vector2 textPosition;

			// calculate screenspace of text3d space position
			var screenSpace = renderArgs.GraphicsDevice.Viewport.Project(Vector3.Zero,
				camera.ProjectionMatrix,
				camera.ViewMatrix,
				Matrix.CreateTranslation(entity.KnownPosition.ToXnaVector3() + new Vector3(0, (float) entity.Height, 0)));


			// get 2D position from screenspace vector
			textPosition.X = screenSpace.X;
			textPosition.Y = screenSpace.Y;

			float s = 0.5f;
			var scale = new Vector2(s, s);

			string clean = entity.NameTag.StripIllegalCharacters();

			var stringCenter = Alex.Font.MeasureString(clean) * s;
			var c = new Point((int) stringCenter.X, (int) stringCenter.Y);

			textPosition.X = (int)(textPosition.X - c.X);
			textPosition.Y = (int)(textPosition.Y - c.Y);

			renderArgs.SpriteBatch.FillRectangle(new Rectangle(textPosition.ToPoint(), c), new Color(Color.Black, 128));
			renderArgs.SpriteBatch.DrawString(Alex.Font, clean, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
	}

	public enum EntityType
	{
		None = 0,

		DroppedItem = 64,
		ExperienceOrb = 69,

		PrimedTnt = 65,
		FallingBlock = 66,

		ThrownBottleoEnchanting = 68,
		EnderEye = 70,
		EnderCrystal = 71,
		FireworksRocket = 72,
		ShulkerBullet = 76,
		FishingRodHook = 77,
		DragonFireball = 79,
		ShotArrow = 80,
		ThrownSnowball = 81,
		ThrownEgg = 82,
		Painting = 83,
		Minecart = 84,
		GhastFireball = 85,
		ThrownSpashPotion = 86,
		ThrownEnderPerl = 87,
		LeashKnot = 88,
		WitherSkull = 89,
		Boat = 90,
		WitherSkullDangerous = 91,
		LightningBolt = 93,
		BlazeFireball = 94,
		AreaEffectCloud = 95,
		HopperMinecart = 96,
		TntMinecart = 97,
		ChestMinecart = 98,
		CommandBlockMinecart = 100,
		LingeringPotion = 101,
		LlamaSpit = 102,
		EvocationFangs = 103,

		Zombie = 32,
		Creeper = 33,
		Skeleton = 34,
		Spider = 35,
		ZombiePigman = 36,
		Slime = 37,
		Enderman = 38,
		Silverfish = 39,
		CaveSpider = 40,
		Ghast = 41,
		MagmaCube = 42,
		Blaze = 43,
		ZombieVillager = 44,
		Witch = 45,
		Stray = 46,
		Husk = 47,
		WitherSkeleton = 48,
		Guardian = 49,
		ElderGuardian = 50,
		Wither = 52,
		Dragon = 53,
		Shulker = 54,
		Endermite = 55,
		Vindicator = 57,
		Evoker = 104,
		Vex = 105,

		Chicken = 10,
		Cow = 11,
		Pig = 12,
		Sheep = 13,
		Wolf = 14,
		Villager = 15,
		MushroomCow = 16,
		Squid = 17,
		Rabbit = 18,
		Bat = 19,
		IronGolem = 20,
		SnowGolem = 21,
		Ocelot = 22,
		Horse = 23,
		Donkey = 24,
		Mule = 25,
		SkeletonHorse = 26,
		ZombieHorse = 27,
		PolarBear = 28,
		Llama = 29,

		Player = 63,

		Npc = 51,
		Agent = 56,
		Camera = 62,
		Chalkboard = 78
	}

	public static class EntityHelpers
	{
		public static Entity CreateEntity(this short entityTypeId, Level world)
		{
			EntityType entityType = (EntityType)entityTypeId;
			return entityType.Create(world);
		}

		public static Entity Create(this EntityType entityType, Level world)
		{
			Entity entity = null;

			switch (entityType)
			{
				case EntityType.None:
					return null;
				case EntityType.Chicken:
					entity = new Chicken(world);
					break;
				case EntityType.Cow:
					entity = new Cow(world);
					break;
				case EntityType.Pig:
					entity = new Pig(world);
					break;
				case EntityType.Sheep:
					entity = new Sheep(world);
					break;
				case EntityType.Wolf:
					entity = new Wolf(world);
					break;
				case EntityType.Villager:
					entity = new Villager(world);
					break;
				case EntityType.MushroomCow:
					entity = new MushroomCow(world);
					break;
				case EntityType.Squid:
					entity = new Squid(world);
					break;
				case EntityType.Rabbit:
					entity = new Rabbit(world);
					break;
				case EntityType.Bat:
					entity = new Bat(world);
					break;
				case EntityType.IronGolem:
					entity = new IronGolem(world);
					break;
				case EntityType.SnowGolem:
					entity = new SnowGolem(world);
					break;
				case EntityType.Ocelot:
					entity = new Ocelot(world);
					break;
				case EntityType.Zombie:
					entity = new Zombie(world);
					break;
				case EntityType.Creeper:
					entity = new Creeper(world);
					break;
				case EntityType.Skeleton:
					entity = new Skeleton(world);
					break;
				case EntityType.Spider:
					entity = new Spider(world);
					break;
				case EntityType.ZombiePigman:
					entity = new ZombiePigman(world);
					break;
				case EntityType.Slime:
					entity = new Slime(world);
					break;
				case EntityType.Enderman:
					entity = new Enderman(world);
					break;
				case EntityType.Silverfish:
					entity = new Silverfish(world);
					break;
				case EntityType.CaveSpider:
					entity = new CaveSpider(world);
					break;
				case EntityType.Ghast:
					entity = new Ghast(world);
					break;
				case EntityType.MagmaCube:
					entity = new MagmaCube(world);
					break;
				case EntityType.Blaze:
					entity = new Blaze(world);
					break;
				case EntityType.ZombieVillager:
					entity = new ZombieVillager(world);
					break;
				case EntityType.Witch:
					entity = new Witch(world);
					break;
				case EntityType.Stray:
					entity = new Stray(world);
					break;
				case EntityType.Husk:
					entity = new Husk(world);
					break;
				case EntityType.WitherSkeleton:
					entity = new WitherSkeleton(world);
					break;
				case EntityType.Guardian:
					entity = new Guardian(world);
					break;
				case EntityType.ElderGuardian:
					entity = new ElderGuardian(world);
					break;
				case EntityType.Horse:
					var random = new Random();
					entity = new Horse(world, random.NextDouble() < 0.10, random);
					break;
				case EntityType.PolarBear:
					entity = new PolarBear(world);
					break;
				case EntityType.Shulker:
					entity = new Shulker(world);
					break;
				case EntityType.Dragon:
					entity = new Dragon(world);
					break;
				case EntityType.SkeletonHorse:
					entity = new SkeletonHorse(world);
					break;
				case EntityType.Wither:
					entity = new Wither(world);
					break;
				case EntityType.Evoker:
					entity = new Evoker(world);
					break;
				case EntityType.Vindicator:
					entity = new Vindicator(world);
					break;
				case EntityType.Vex:
					entity = new Vex(world);
					break;
				case EntityType.Npc:
					entity = new PlayerMob("test", world);
					break;
				default:
					return null;
			}

			return entity;
		}
	}
}
