using System;
using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	internal class MobsModelDefinition : Dictionary<string, EntityModel>
	{
		
	}
	
	internal class MobsModelConverter : JsonConverter
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		private EntityModel DecodeSingle(JObject jObject, JsonSerializer serializer)
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
		
		private IEnumerable<EntityModel> Decode1120(JObject jObject, JsonSerializer serializer)
		{
			//EntityModel model = new EntityModel();
			//model.Description = new ModelDescription();
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
						foreach (var geo in ((JArray)property).AsJEnumerable())
						{
							if (geo.Type == JTokenType.Object)
							{
								yield return DecodeSingle((JObject) geo, serializer);
							}
						}
					}
					else if (property.Type == JTokenType.Object)
					{
						yield return DecodeSingle((JObject) property, serializer);
					}
				}
			}

			//return model;
		}
		
		private IEnumerable<EntityModel> Decode180(JObject jObject, JsonSerializer serializer)
		{
			//EntityModel model = new EntityModel();
			//model.Description = new ModelDescription();
			foreach (var prop in jObject)
			{
				if (prop.Key.Equals("format_version"))
					continue;

				var property = prop.Value;

				if (property == null)
					continue;


			/*	if (property.Type == JTokenType.Array)
				{
					foreach (var geo in property.Values())
					{
						if (geo.Type == JTokenType.Object)
						{
							yield return DecodeSingle((JObject) geo, serializer);
						}
					}
				}
				else */if (property.Type == JTokenType.Object)
				{
					var singleDecode = DecodeSingle((JObject) property, serializer);

					if (singleDecode != null)
					{
						singleDecode.Description.Identifier = prop.Key;

						yield return singleDecode;
					}
				}
			}

			//return model;
		}
		
		/// <inheritdoc />
		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type != JTokenType.Object) 
				return null;
			
			var                 jObject = (JObject)obj;
			MobsModelDefinition result  = new MobsModelDefinition();
			if (jObject.TryGetValue(
				"format_version", StringComparison.InvariantCultureIgnoreCase, out var versionToken))
			{
				switch (versionToken.Value<string>())
				{
					case "1.8.0":
					{
						foreach (var model in Decode180(jObject, serializer))
						{
							result.TryAdd(model.Description.Identifier, model);
						}

						break;
					}

					//case "1.10.0":
					//	return Decode110(jObject, serializer);
					case "1.12.0":
					{
						foreach (var model in Decode1120(jObject, serializer))
						{
							result.TryAdd(model.Description.Identifier, model);
						}
						
						break;
					}
				}
			}
			
			//model.Description.Identifier = obj.
			/*foreach (var prop in jObject)
			{
				var property = prop.Value;

				if (property == null)
					continue;
				
				if (prop.Key.Equals("format_version", StringComparison.InvariantCultureIgnoreCase))
					continue;

				var model = property.ToObject<EntityModel>(serializer);
				result.TryAdd(prop.Key, model);
			}*/

			return result;
		}

		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(MobsModelDefinition).IsAssignableFrom(objectType);
		}
	}
}