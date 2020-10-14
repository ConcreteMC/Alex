using System;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class EntityModelConverter : JsonConverter
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type != JTokenType.Object) 
				return null;
			
			EntityModel model   = new EntityModel();
			model.Description = new ModelDescription();
			var         jObject = (JObject)obj;
			//model.Description.Identifier = obj.
			foreach (var prop in jObject)
			{
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

		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(EntityModel).IsAssignableFrom(objectType);
		}
	}
}