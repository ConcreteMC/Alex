using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils;
using Alex.Entities.Generic;
using Alex.Entities.Hostile;
using Alex.Entities.Passive;
using Alex.Entities.Projectiles;
using Alex.Entities.Vehicles;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Worlds;
using fNbt;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Entities;
using Newtonsoft.Json;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using PlayerLocation = Alex.Common.Utils.Vectors.PlayerLocation;
using ResourceLocation = Alex.Common.Resources.ResourceLocation;

namespace Alex.Entities
{
	public static class EntityFactory
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityFactory));

		private static ConcurrentDictionary<string, Func<EntityModelRenderer>> _registeredRenderers =
			new ConcurrentDictionary<string,  Func<EntityModelRenderer>>();

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
                //p.Name = p.Name.Replace("_", "");
                
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

		public static bool ModelByNetworkId(long networkId, out EntityData data)
		{
			if (_idToData.TryGetValue(networkId, out data))
			{
				return true;
				//renderer = TryGetRendererer(data);
				//if (renderer != null)
				//{
				//	return true;
				//}
				//else
				//{
				//	if (data.OriginalName.Equals("armor_stand"))
				//Log.Warn($"No entity model renderer found for {data.Name} - {data.OriginalName}");
				//}
			}

		//	renderer = null;
			return false;
		}

		private static EntityModelRenderer TryGetRendererer(EntityData data)
		{
			string lookupName = data.OriginalName;

			if (lookupName == "firework_rocket")
			{
				lookupName = "fireworks_rocket";
			}
			
			if (_registeredRenderers.TryGetValue(lookupName, out var func))
			{
				return func();
			}
			else
			{
				var f = _registeredRenderers.Where(x => x.Key.Length >= data.OriginalName.Length)
				   .OrderBy(x => (x.Key.Length - data.OriginalName.Length)).FirstOrDefault(
						x => x.Key.ToString().ToLowerInvariant().Contains(data.OriginalName.ToLowerInvariant())).Value;

				if (f != null)
				{
					return f();
				}
			}

			return null;
		}

		public static EntityModelRenderer GetEntityRenderer(string name)
		{
			if (_registeredRenderers.TryGetValue(name, out var func))
			{
				if (func != null) return func();
			}
			else
			{
				var f = _registeredRenderers.FirstOrDefault(x => x.Key.ToString().ToLowerInvariant().Contains(name.ToLowerInvariant())).Value;

				if (f != null)
				{
					return f();
				}
			}
			return null;
		}

		public static void Reset()
		{
			var registered = _registeredRenderers.ToArray();
			_registeredRenderers.Clear();

			//foreach (var kv in registered)
			//{
			//	EntityModelRenderer.Remove(kv.Key);
			//}

			var textures = _pooledTextures.ToArray();
			_pooledTextures.Clear();

			foreach (var texture in textures)
			{
				if (!texture.Value.IsDisposed)
					texture.Value?.Dispose();
			//	if (texture.Value != null)
				//	texture.Value.Dispose();
			}
		}

		public static int LoadEntityDefinitions(BedrockResourcePack resourcePack,
			bool replaceModels,
			IProgressReceiver progressReceiver = null)
		{
			var entityDefinitions = resourcePack.EntityDefinitions;
			var beforeImport = _registeredRenderers.Count;
			int done = 0;
			int total = entityDefinitions.Count;

			foreach (var def in entityDefinitions.OrderByDescending(
				x => string.IsNullOrWhiteSpace(x.Value.MinEngineVersion)))
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

					Func<EntityModelRenderer> previousValue = null;

					/*	if (_registeredRenderers.TryGetValue(def.Value.Identifier, out previousValue))
						{
							if (replaceModels)
							{
								_registeredRenderers.Remove(def.Value.Identifier, out _);
							}
							else
							{
								continue;
							}
						}*/

					string modelKey;

					if (!geometry.TryGetValue("default", out modelKey) && !geometry.TryGetValue(
						new ResourceLocation(def.Value.Identifier).Path, out modelKey))
					{
						modelKey = geometry.FirstOrDefault().Value;
					}

					EntityModel model;

					if (ModelFactory.TryGetModel(modelKey, out model) && model != null)
					{
						if (_registeredRenderers.TryGetValue(def.Value.Identifier, out previousValue) && !replaceModels)
							continue;

						_registeredRenderers[def.Value.Identifier] = () =>
						{
							if (EntityModelRenderer.TryGetRenderer(model, out var renderer))
							{
								return renderer;
							}

							return null;
						};
						//Add(def.Value, model);
						//Add(resources, graphics, def.Value, model, def.Key.ToString());
					}
					//else 
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
			return _registeredRenderers.Count - beforeImport;
		}

		private static ConcurrentDictionary<string, Texture2D> _pooledTextures =
			new ConcurrentDictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

		public static readonly Guid PooledTagIdentifier = Guid.Parse("58142097-1b3d-4dc5-b98e-33da88bcf74a");
		public static Entity Create(ResourceLocation entityType, World world, bool initRenderController = true)
		{
			Entity entity = null;

			if (entityType.Namespace == ResourceLocation.DefaultNamespace)
			{
				switch (MiNET.Entities.EntityHelpers.ToEntityType(entityType.ToString()))
				{
					//	case MiNET.Entities.EntityType.None:
					//return null;
					case MiNET.Entities.EntityType.Chicken:
						entity = new Chicken(world);

						break;

					case MiNET.Entities.EntityType.Cow:
						entity = new Cow(world);

						break;

					case MiNET.Entities.EntityType.Pig:
						entity = new Pig(world);

						break;

					case MiNET.Entities.EntityType.Sheep:
						entity = new Sheep(world);

						break;

					case MiNET.Entities.EntityType.Wolf:
						entity = new Wolf(world);

						break;

					case MiNET.Entities.EntityType.Villager:
						entity = new Villager(world);

						break;

					case MiNET.Entities.EntityType.MushroomCow:
						entity = new Mooshroom(world);

						break;

					case MiNET.Entities.EntityType.Squid:
						entity = new Squid(world);

						break;

					case MiNET.Entities.EntityType.Rabbit:
						entity = new Rabbit(world);

						break;

					case MiNET.Entities.EntityType.Bat:
						entity = new Bat(world);

						break;

					case MiNET.Entities.EntityType.IronGolem:
						entity = new VillagerGolem(world);

						break;

					case MiNET.Entities.EntityType.SnowGolem:
						entity = new Snowman(world);

						break;

					case MiNET.Entities.EntityType.Ocelot:
						entity = new Ocelot(world);

						break;

					case MiNET.Entities.EntityType.Zombie:
						entity = new Zombie(world);

						break;

					case MiNET.Entities.EntityType.Creeper:
						entity = new Creeper(world);

						break;

					case MiNET.Entities.EntityType.Skeleton:
						entity = new Skeleton(world);

						break;

					case EntityType.Spider:
						entity = new Spider(world);

						break;

					case MiNET.Entities.EntityType.ZombiePigman:
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

					case EntityType.Dragon:
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
						entity = new EntityArmorStand(world);

						break;

					case EntityType.ShotArrow:
						entity = new ArrowEntity(world);

						break;

					case EntityType.DroppedItem:
						entity = new ItemEntity(world);

						break;

					case EntityType.ThrownSnowball:
						entity = new SnowballEntity(world);

						break;

					case EntityType.ThrownEgg:
						entity = new EggEntity(world);

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

					case (EntityType)121: //Fox
						entity = new Fox(world);

						break;

					case (EntityType)122:
						entity = new Bee(world);

						break;

					case EntityType.Parrot:
						entity = new Parrot(world);

						break;

					case EntityType.Phantom:
						entity = new Phantom(world);

						break;

					case EntityType.LlamaSpit:
						entity = new LlamaSpit(world);

						break;

					case EntityType.Cat:
						entity = new Cat(world);

						break;

					case EntityType.BlazeFireball:
						entity = new SmallFireball(world);

						break;

					case EntityType.GhastFireball:
						entity = new Fireball(world);

						break;

					case EntityType.Dolphin:
						entity = new Dolphin(world);

						break;

					case (EntityType)112:
						entity = new Cod(world);

						break;

					case EntityType.Pufferfish:
						entity = new PufferFish(world);

						break;

					case EntityType.Salmon:
						entity = new Salmon(world);

						break;

					case EntityType.TropicalFish:
						entity = new TropicalFish(world);

						break;

					case EntityType.FireworksRocket:
						entity = new FireworkRocket(world);

						break;

					case EntityType.Player:
						entity = new RemotePlayer(world);

						break;

					case EntityType.PrimedTnt:
						entity = new PrimedTntEntity(world);

						break;

					case EntityType.ThrownEnderPerl:
						entity = new ThrownEnderPearlEntity(world);

						break;

					case EntityType.EnderEye:
						entity = new ThrownEyeOfEnderEntity(world);

						break;

					case EntityType.Minecart:
						entity = new MinecartEntity(world);

						break;

					case EntityType.TntMinecart:
						entity = new TntMinecartEntity(world);

						break;

					case EntityType.ChestMinecart:
						entity = new ChestMinecartEntity(world);

						break;

					case EntityType.HopperMinecart:
						entity = new HopperMinecartEntity(world);

						break;

					case EntityType.AreaEffectCloud:
						entity = new EntityAreaEffectCloud(world);

						break;

					case EntityType.ExperienceOrb:
						entity = new XpOrbEntity(world);

						break;
					//case EntityType.Human:
					//entity = new PlayerMob("test", world, );
					//	break;
					//default:
					//	return null;
				}
			}

			//entity = new Entity(world);

			//var stringId = entityType.ToStringId();
			var resources = Alex.Instance.Resources;

			if (resources.TryGetEntityDefinition(entityType, out var description, out var resourcePack))
			{
				if (entity == null)
				{
					//Log.Warn($"No entity implementation found, falling back to Alex.Entities.Entity for: {entityType}");
					entity = new Entity(world);
				}

				world.BackgroundWorker.Enqueue(
					() =>
					{
						if (initRenderController)
						{
							entity.AnimationController.UpdateEntityDefinition(resourcePack, description);
						}

						var modelRenderer = GetEntityRenderer(description.Identifier);

						if (modelRenderer == null)
						{
							Log.Warn($"Missing entity renderer: {entityType}");
						}

						entity.ModelRenderer = modelRenderer;


						//Texture2D texture2D = null;
						//if (renderer == null || texture2D == null)
						{
							if (description.Geometry.TryGetValue("default", out var defaultGeometry)
							    && ModelFactory.TryGetModel(defaultGeometry, out var model) && model != null)
							{
								var textures = description.Textures;
								string texture;

								if (!(textures.TryGetValue("default", out texture) || textures.TryGetValue(
									description.Identifier, out texture)))
								{
									texture = textures.FirstOrDefault().Value;
								}

								if (!_pooledTextures.TryGetValue(texture, out var texture2D))
								{
									if (resourcePack.TryGetBitmap(texture, out var bmp))
									{
										Alex.Instance.UiTaskManager.Enqueue(
											() =>
											{
												texture2D = TextureUtils.BitmapToTexture2D(
													entity, Alex.Instance.GraphicsDevice, bmp);

												texture2D.Tag = EntityFactory.PooledTagIdentifier;

												texture2D.Disposing += (sender, args) =>
												{
													_pooledTextures.TryRemove(texture, out _);
												};

												_pooledTextures.TryAdd(texture, texture2D);

												entity.Texture = texture2D;
											});
									}
								}
								else
								{
									entity.Texture = texture2D;
								}
							}
						}
					});
			}

			return entity;
		}
	}
}
