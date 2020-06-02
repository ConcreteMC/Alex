using Alex.Entities;
using Alex.Entities.Hostile;
using Alex.Entities.Passive;
using Alex.Worlds;

namespace Alex.Utils
{
	public static class EntityHelpers
	{
		public static Entity Create(this EntityType entityType, World world)
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
					entity = new Mooshroom(world);
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
					entity = new VillagerGolem(world);
					break;
				case EntityType.SnowGolem:
					entity = new Snowman(world);
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
					entity = new Horse(world);
					break;
				case EntityType.PolarBear:
					entity = new PolarBear(world);
					break;
				case EntityType.Shulker:
					entity = new Shulker(world);
					break;
				case EntityType.EnderDragon:
					entity = new EnderDragon(world);
					break;
				case EntityType.SkeletonHorse:
					entity = new SkeletonHorse(world);
					break;
				case EntityType.Wither:
					entity = new Wither(world);
					break;
				case EntityType.Evoker:
					entity = new EvocationIllager(world);
					break;
				case EntityType.Vindicator:
					entity = new VindicationIllager(world);
					break;
				case EntityType.Vex:
					entity = new Vex(world);
					break;
				case EntityType.FallingBlock:
					entity = new EntityFallingBlock(world, null);
					break;
				//case EntityType.Human:
					//entity = new PlayerMob("test", world, );
				//	break;
				default:
					return null;
			}

			return entity;
		}
	}
}