using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Alex.API.Utils;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using fNbt;
using fNbt.Tags;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Net;
using MiNET.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Entities
{
	public static class EntityFactory
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityFactory));

		private static ConcurrentDictionary<string, Func<Texture2D, EntityModelRenderer>> _registeredRenderers =
			new ConcurrentDictionary<string, Func<Texture2D, EntityModelRenderer>>();

		private static IReadOnlyDictionary<long, EntityData> _idToData;
		public static void Load(ResourceManager resourceManager, IProgressReceiver progressReceiver)
		{
			progressReceiver?.UpdateProgress(0, "Loading entity data...");

            Dictionary<long, EntityData> networkIdToData = new Dictionary<long, EntityData>();

			EntityData[] entityObjects = JsonConvert.DeserializeObject<EntityData[]>(Resources.NewEntities);

            for (int i = 0; i < entityObjects.Length; i++)
			{
                EntityData p = entityObjects[i];

				progressReceiver?.UpdateProgress(100 * (i / entityObjects.Length), "Loading entity data...", p.Name);
				if (resourceManager.Registries.Entities.Entries.TryGetValue($"minecraft:{p.Name}",
					out var registryEntry))
				{
					networkIdToData.TryAdd(registryEntry.ProtocolId, p);
                    //networkIdToData.TryAdd(p.InternalId + 1, p);
                }
				else
				{
					Log.Warn($"Could not resolve {p.Name}'s protocol id!");
				}

				/*foreach (var dd in p)
				{
					EntityData data = new EntityData();
					data.Id = dd["id"].Value<long>();
					data.Name = dd["name"].Value<string>();

					data.DisplayName = dd["display_name"]?.Value<string>();

					if (dd["height"] != null) data.Height = dd["height"].Value<double>();
					if (dd["width"] != null) data.Width = dd["width"].Value<double>();

					networkIdToData.TryAdd(data.Id, data);
				}*/
            }

			_idToData = networkIdToData;
		}

		public static bool TryLoadEntity(NbtCompound nbt, long entityId, out Entity entity)
		{
			var id = nbt["id"].StringValue;
			var pos = nbt["Pos"];
			var rot = nbt["Rotation"];
			if (id != null && pos != null && EntityType.TryParse(id.Replace("minecraft:", ""), true, out EntityType entityType))
			{
				var uuidLeast = nbt["UUIDLeast"].LongValue;
				var uuidMost = nbt["UUIDMost"].LongValue;

				Guid uuid = Extensions.GuidFromBits(uuidLeast, uuidMost);

				var renderer = GetEntityRenderer(id, null);
				if (renderer != null)
				{
					entity = entityType.Create(null);
					if (entity == null) return false;

					entity.EntityId = entityId;
					entity.UUID = new UUID(uuid.ToByteArray());

					PlayerLocation position = new PlayerLocation(Convert.ToSingle(pos[0].DoubleValue), Convert.ToSingle(pos[1].DoubleValue),
						Convert.ToSingle(pos[2].DoubleValue), rot[0].FloatValue, rot[0].FloatValue, rot[1].FloatValue);

					entity.KnownPosition = position;

					entity.ModelRenderer = renderer;

					return true;
				}
			}

			entity = null;
			return false;
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
					Log.Warn($"No renderer found for {data.Name}");
				}
			}

			renderer = null;
			return false;
		}

		private static EntityModelRenderer TryGetRendererer(EntityData data, Texture2D texture)
		{
			if (_registeredRenderers.TryGetValue(data.Name, out var func))
			{
				return func(texture);
			}
			else
			{
				var f = _registeredRenderers.FirstOrDefault(x => x.Key.ToLowerInvariant().Contains(data.Name.ToLowerInvariant())).Value;

				if (f != null)
				{
					return f(texture);
				}
			}
			return null;
		}

		public static EntityModelRenderer GetEntityRenderer(string name, Texture2D texture)
		{
			if (_registeredRenderers.TryGetValue(name, out var func))
			{
				if (func != null) return func(texture);
			}
			else
			{
				var f = _registeredRenderers.FirstOrDefault(x => x.Key.ToLowerInvariant().Contains(name.ToLowerInvariant())).Value;

				if (f != null)
				{
					return f(texture);
				}
			}
			return null;
		}
		
		public static void LoadModels(ResourceManager resourceManager, GraphicsDevice graphics, bool replaceModels, IProgressReceiver progressReceiver = null)
		{
			var entityDefinitions = resourceManager.BedrockResourcePack.EntityDefinitions;
			int done = 0;
			int total = entityDefinitions.Count;

			foreach (var def in entityDefinitions)
			{
				double percentage = 100D * ((double)done / (double)total);
				progressReceiver?.UpdateProgress((int)percentage, $"Importing entity definitions...", def.Key);

                try
				{
					if (def.Value.Textures == null) continue;
					if (def.Value.Geometry == null) continue;
					if (def.Value.Textures.Count == 0) continue;
					if (def.Value.Geometry.Count == 0) continue;

					EntityModel model;
					if ((resourceManager.BedrockResourcePack.EntityModels.TryGetValue(def.Value.Geometry["default"],
						    out model)) && model != null)
					{
						Add(resourceManager, graphics, def.Value, model, def.Value.Filename);
						Add(resourceManager, graphics, def.Value, model, def.Key);
					}
					else if ((resourceManager.BedrockResourcePack.EntityModels.TryGetValue(def.Value.Geometry["default"] + ".v1.8",
						         out model)) && model != null)
					{
						Add(resourceManager, graphics, def.Value, model, def.Value.Filename);
						Add(resourceManager, graphics, def.Value, model, def.Key);
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
		}

		private static void Add(ResourceManager resourceManager, GraphicsDevice graphics, BedrockResourcePack.EntityDefinition def, EntityModel model, string name)
		{
			_registeredRenderers.AddOrUpdate(name,
				(t) =>
				{
					if (t == null)
					{
						var textures = def.Textures;
						string texture;
						if (!textures.TryGetValue("default", out texture))
						{
							texture = textures.FirstOrDefault().Value;
						}

						if (resourceManager.BedrockResourcePack.Textures.TryGetValue(texture,
							out Bitmap bmp))
						{
							t = TextureUtils.BitmapToTexture2D(graphics, bmp);
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
						if (!textures.TryGetValue("default", out texture))
						{
							texture = textures.FirstOrDefault().Value;
						}

						if (resourceManager.BedrockResourcePack.Textures.TryGetValue(texture,
							out Bitmap bmp))
						{
							t = TextureUtils.BitmapToTexture2D(graphics, bmp);
						}

						return new EntityModelRenderer(model, t);
					};
				});
		}
	}
}
