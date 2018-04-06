using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.API.Utils;
using Alex.Graphics.Models;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using fNbt.Tags;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Entities
{
	public static class EntityFactory
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EntityFactory));

		private static ConcurrentDictionary<string, Func<Texture2D, EntityModelRenderer>> _registeredRenderers =
			new ConcurrentDictionary<string, Func<Texture2D, EntityModelRenderer>>();

		public static void Load()
		{

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

		public static EntityModelRenderer GetEntityRenderer(string name, Texture2D texture)
		{
			if (_registeredRenderers.TryGetValue(name, out var func))
			{
				return func(texture);
			}
			else
			{
				var f = _registeredRenderers.FirstOrDefault(x => x.Key.Contains(name));
				if (f.Value != null)
				{
					return f.Value(texture);
				}
			}
			return null;
		}

		public static void LoadModels(ResourceManager resourceManager, GraphicsDevice graphics, bool replaceModels)
		{
			foreach (var def in resourceManager.BedrockResourcePack.EntityDefinitions)
			{
				try
				{
					if (def.Value.Textures == null) continue;
					if (def.Value.Geometry == null) continue;
					if (def.Value.Textures.Count == 0) continue;
					if (def.Value.Geometry.Count == 0) continue;

					EntityModel model;
					if (resourceManager.BedrockResourcePack.EntityModels.TryGetValue(def.Value.Geometry["default"],
						    out model) && model != null)
					{
						_registeredRenderers.AddOrUpdate(def.Key,
							(t) =>
							{
								if (t == null)
								{
									var textures = def.Value.Textures;
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
									var textures = def.Value.Textures;
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
						//	}
					}
				}
				catch (Exception ex)
				{
					Log.Warn(ex, $"Failed to load model {def.Key}!");
				}
			}
		}
	}
}
