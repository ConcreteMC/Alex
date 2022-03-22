using System;
using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class EntityModelConverter : JsonConverter
	{
		private static ILogger Log = LogManager.GetCurrentClassLogger();

		public EntityModelConverter() { }

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		private EntityModel Decode(JObject jObject, JsonSerializer serializer)
		{
			EntityModel model = new EntityModel();
			model.Description = new ModelDescription();

			foreach (var prop in jObject)
			{
				if (prop.Key.Equals("format_version"))
					continue;

				var property = prop.Value;

				if (property == null)
					continue;

				if (property.Type == JTokenType.Integer || property.Type == JTokenType.Float)
				{
					switch (prop.Key.ToLower())
					{
						case "texturewidth":
							model.Description.TextureWidth = property.Value<long>();

							break;

						case "textureheight":
							model.Description.TextureHeight = property.Value<long>();

							break;

						case "visible_bounds_width":
							model.Description.VisibleBoundsWidth = property.Value<double>();

							break;

						case "visible_bounds_height":
							model.Description.VisibleBoundsHeight = property.Value<double>();

							break;
					}
				}
				else if (property.Type == JTokenType.Array)
				{
					switch (prop.Key.ToLower())
					{
						case "visible_bounds_offset":
							model.Description.VisibleBoundsOffset = property.ToObject<Vector3>(serializer);

							break;

						case "bones":
						{
							model.Bones = property.ToObject<EntityModelBone[]>(serializer);

							break;
						}
					}
				}
				else if (property.Type == JTokenType.Object)
				{
					switch (prop.Key.ToLower())
					{
						case "description":
							model.Description = property.ToObject<ModelDescription>(serializer);

							break;
					}
				}
			}

			return model;
		}

		private EntityModel Decode180(JObject jObject, JsonSerializer serializer)
		{
			EntityModel model = new EntityModel();
			model.Description = new ModelDescription();

			foreach (var prop in jObject)
			{
				if (prop.Key.Equals("format_version"))
					continue;

				var property = prop.Value;

				if (property == null)
					continue;

				if (property.Type == JTokenType.Integer || property.Type == JTokenType.Float)
				{
					switch (prop.Key.ToLower())
					{
						case "texturewidth":
							model.Description.TextureWidth = property.Value<long>();

							break;

						case "textureheight":
							model.Description.TextureHeight = property.Value<long>();

							break;

						case "visible_bounds_width":
							model.Description.VisibleBoundsWidth = property.Value<double>();

							break;

						case "visible_bounds_height":
							model.Description.VisibleBoundsHeight = property.Value<double>();

							break;
					}
				}
				else if (property.Type == JTokenType.Array)
				{
					switch (prop.Key.ToLower())
					{
						case "visible_bounds_offset":
							model.Description.VisibleBoundsOffset = property.ToObject<Vector3>(serializer);

							break;

						case "bones":
						{
							model.Bones = property.ToObject<EntityModelBone[]>(serializer);

							break;
						}
					}
				}
				else if (property.Type == JTokenType.Object)
				{
					switch (prop.Key.ToLower())
					{
						case "description":
							model.Description = property.ToObject<ModelDescription>(serializer);

							break;
					}
				}
			}

			return model;
		}

		private EntityModel Decode112(JObject jObject, JsonSerializer serializer)
		{
			EntityModel model = new EntityModel();
			model.Description = new ModelDescription();

			foreach (var prop in jObject)
			{
				if (prop.Key.Equals("format_version"))
					continue;

				var property = prop.Value;

				if (property == null)
					continue;

				if (prop.Key.Equals("minecraft:geometry"))
				{
					if (property.Type == JTokenType.Array)
					{
						foreach (var geo in property.Values())
						{
							if (geo.Type == JTokenType.Object)
							{
								model = Decode180((JObject)geo, serializer);
							}
						}
					}
				}
			}

			return model;
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type != JTokenType.Object)
				return null;

			//var jObject = (JObject)obj;

			//	return Decode180(jObject, serializer);
			var jObject = (JObject)obj;

			if (jObject.TryGetValue(
				    "format_version", StringComparison.InvariantCultureIgnoreCase, out var versionToken))
			{
				switch (versionToken.Value<string>())
				{
					case "1.8.0":
						return Decode180(jObject, serializer);

					//case "1.10.0":
					//	return Decode110(jObject, serializer);
					case "1.12.0":
						return Decode112(jObject, serializer);

					default:
						Log.Warn($"Invalid format version: {versionToken.Value<string>()}");

						break;
				}
			}

			return Decode180(jObject, serializer);
		}

		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(EntityModel).IsAssignableFrom(objectType);
		}
	}
}