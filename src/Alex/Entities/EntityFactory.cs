using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Alex.API.Graphics;
using Alex.API.Resources;
using Alex.API.Utils;
using Alex.Entities.Generic;
using Alex.Entities.Hostile;
using Alex.Entities.Passive;
using Alex.Entities.Projectiles;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using NLog;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using ResourceLocation = Alex.API.Resources.ResourceLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Entities
{
	public static class EntityFactory
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityFactory));

		private static ConcurrentDictionary<ResourceLocation, Func<PooledTexture2D, EntityModelRenderer>> _registeredRenderers =
			new ConcurrentDictionary<ResourceLocation, Func<PooledTexture2D, EntityModelRenderer>>();

		private static IReadOnlyDictionary<long, EntityData> _idToData;
		public static void Load(ResourceManager resourceManager, IProgressReceiver progressReceiver)
		{
			progressReceiver?.UpdateProgress(0, "Loading entity data...");

            Dictionary<long, EntityData> networkIdToData = new Dictionary<long, EntityData>();
            EntityData[] entityObjects = JsonConvert.DeserializeObject<EntityData[]>(ResourceManager.ReadStringResource("Alex.Resources.NewEntities.txt"));

            long unknownId = 0;
            for (int i = 0; i < entityObjects.Length; i++)
			{
                EntityData p = entityObjects[i];
                var originalName = p.Name;
                p.OriginalName = originalName;
                p.Name = p.Name.Replace("_", "");
                
                long id = 0;
				progressReceiver?.UpdateProgress(i, entityObjects.Length, "Loading entity data...", p.Name);
				if (resourceManager.Registries.Entities.Entries.TryGetValue($"minecraft:{originalName}",
					out var registryEntry))
                {
                    id = registryEntry.ProtocolId;
					networkIdToData.TryAdd(registryEntry.ProtocolId, p);
                    //networkIdToData.TryAdd(p.InternalId + 1, p);
                }
				else
				{
					Log.Warn($"Could not resolve {p.Name}'s protocol id!");
                    id = unknownId++;
                }
			}

			_idToData = networkIdToData;
        }

		public static bool ModelByNetworkId(long networkId, out EntityModelRenderer renderer, out EntityData data)
		{
			if (_idToData.TryGetValue(networkId, out data))
			{
				renderer = TryGetRendererer(data, null);
				if (renderer != null)
				{
					return true;
                }
				else
				{
				//	if (data.OriginalName.Equals("armor_stand"))
						//Log.Warn($"No entity model renderer found for {data.Name} - {data.OriginalName}");
				}
			}

			renderer = null;
			return false;
		}

		private static EntityModelRenderer TryGetRendererer(EntityData data, PooledTexture2D texture)
		{
			string lookupName = data.OriginalName;

			if (lookupName == "firework_rocket")
			{
				lookupName = "fireworks_rocket";
			}
			
			if (_registeredRenderers.TryGetValue(lookupName, out var func))
			{
				return func(texture);
			}
			else
			{
				var f = _registeredRenderers.Where(x => x.Key.Path.Length >= data.OriginalName.Length)
				   .OrderBy(x => (x.Key.Path.Length - data.OriginalName.Length)).FirstOrDefault(
						x => x.Key.ToString().ToLowerInvariant().Contains(data.OriginalName.ToLowerInvariant())).Value;

				if (f != null)
				{
					return f(texture);
				}
			}

			return null;
		}

		public static EntityModelRenderer GetEntityRenderer(string name, PooledTexture2D texture)
		{
			if (_registeredRenderers.TryGetValue(name, out var func))
			{
				if (func != null) return func(texture);
			}
			else
			{
				var f = _registeredRenderers.FirstOrDefault(x => x.Key.ToString().ToLowerInvariant().Contains(name.ToLowerInvariant())).Value;

				if (f != null)
				{
					return f(texture);
				}
			}
			return null;
		}
		
		public static int LoadModels(BedrockResourcePack resourcePack, GraphicsDevice graphics, bool replaceModels, IProgressReceiver progressReceiver = null)
		{
			var entityDefinitions = resourcePack.EntityDefinitions;
			int done              = 0;
			int total             = entityDefinitions.Count;

			foreach (var def in entityDefinitions)
			{
			//	double percentage = 100D * ((double)done / (double)total);
				progressReceiver?.UpdateProgress(done, total, $"Importing entity definitions...", def.Key.ToString());

                try
				{
					if (def.Value.Textures == null) continue;
					if (def.Value.Geometry == null) continue;
					if (def.Value.Textures.Count == 0) continue;
					if (def.Value.Geometry.Count == 0) continue;

					var geometry = def.Value.Geometry;
					string modelKey;
					if (!geometry.TryGetValue("default", out modelKey) && !geometry.TryGetValue(new ResourceLocation(def.Value.Identifier).Path, out modelKey))
					{
						modelKey = geometry.FirstOrDefault().Value;
					}

					EntityModel model;
					if (ModelFactory.TryGetModel(modelKey + ".v1.8", out model) && model != null)
					{
						Add(resourcePack, graphics, def.Value, model, def.Value.Identifier);
						Add(resourcePack, graphics, def.Value, model, def.Key.ToString());
					}
				    else if (ModelFactory.TryGetModel(modelKey, out model) && model != null)
				    {
				        Add(resourcePack, graphics, def.Value, model, def.Value.Identifier);
				        Add(resourcePack, graphics, def.Value, model, def.Key.ToString());
                    }
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Failed to load model {def.Key}!");
				}
				finally
                {
	                done++;
                }
			}

			if (_registeredRenderers.TryGetValue("minecraft:armor_stand", out var func))
				_registeredRenderers.TryAdd("minecraft:armorstand", func);
			
		//    Log.Info($"Registered {(Assembly.GetExecutingAssembly().GetTypes().Count(t => t.Namespace == "Alex.Entities.Models"))} entity models");
		   // Log.Info($"Loaded {_registeredRenderers.Count} entity models");
		   return _registeredRenderers.Count;
		}

		private static void Add(BedrockResourcePack resourcepack, GraphicsDevice graphics, EntityDescription def, EntityModel model, ResourceLocation name)
		{
			_registeredRenderers.AddOrUpdate(name,
				(t) =>
				{
					if (t == null)
					{
						var textures = def.Textures;
						string texture;
						if (!textures.TryGetValue("default", out texture) && !textures.TryGetValue(name.Path, out texture))
						{
							texture = textures.FirstOrDefault().Value;
						}

						if (resourcepack.Textures.TryGetValue(texture,
							out var bmp))
						{
							t = TextureUtils.BitmapToTexture2D(graphics, bmp.Value);
						}
					}

					return new EntityModelRenderer(model, t);
				},
				(s, func) =>
				{
					return (t) =>
					{
						var textures = def.Textures;
						string texture;
						if (!(textures.TryGetValue("default", out texture) || textures.TryGetValue(name.Path, out texture)))
						{
							texture = textures.FirstOrDefault().Value;
						}

						if (resourcepack.Textures.TryGetValue(texture,
							out var bmp))
						{
							t = TextureUtils.BitmapToTexture2D(graphics, bmp.Value);
						}

						return new EntityModelRenderer(model, t);
					};
				});
		}
		
		public static Entity Create(EntityType entityType, World world)
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
				case EntityType.Endermite:
					entity = new Endermite(world);
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
				case EntityType.ZombieHorse:
					entity = new ZombieHorse(world);
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
					entity = new EntityFallingBlock(world);
					break;
				case EntityType.ArmorStand:
					entity = new EntityArmorStand(world, null);
					break;
				case EntityType.Arrow:
					entity = new ArrowEntity(world, null);
					break;
				case EntityType.Item:
					entity = new ItemEntity(world);
					break;
				case EntityType.Mooshroom:
					entity = new Mooshroom(world);
					break;
				case EntityType.Snowball:
					entity = new SnowballEntity(world, null);
					break;
				case EntityType.ThrownEgg:
					entity = new EggEntity(world, null);

					break;
				case EntityType.Donkey:
					entity = new Donkey(world);
					break;
				case EntityType.Llama:
					entity = new Llama(world);
					break;
				case EntityType.Mule:
					entity = new Mule(world);
					break;
				case EntityType.Fox:
					entity = new Fox(world);
					break;
				case EntityType.Parrot:
					entity = new Parrot(world);
					break;
				case EntityType.Phantom:
					entity = new Phantom(world);
					break;
				case EntityType.Bee:
					entity = new Bee(world);
					break;
				case EntityType.LlamaSpit:
					entity = new LlamaSpit(world);
					break;
				case EntityType.Cat:
					entity = new Cat(world);
					break;
				case EntityType.SmallFireball:
					entity = new SmallFireball(world);
					break;
				case EntityType.Fireball:
					entity = new Fireball(world);
					break;
				case EntityType.Dolphin:
					entity = new Dolphin(world);
					break;
				case EntityType.Cod:
					entity = new Cod(world);
					break;
				case EntityType.PufferFish:
					entity = new PufferFish(world);
					break;
				case EntityType.Salmon:
					entity = new Salmon(world);
					break;
				case EntityType.TropicalFish:
					entity = new TropicalFish(world);
					break;
				
				case EntityType.FireworkRocket:
					entity = new FireworkRocket(world, null);
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
