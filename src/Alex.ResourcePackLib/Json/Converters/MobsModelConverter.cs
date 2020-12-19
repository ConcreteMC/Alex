using System;
using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.ResourcePackLib.Json.Converters
{
	[JsonConverter(typeof(MobsModelConverter))]
	internal class MobsModelDefinition : Dictionary<string, EntityModel>
	{
		
	}
	
	internal class MobsModelConverter : JsonConverter
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MobsModelConverter));
		
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
		}
		
		private IEnumerable<EntityModel> Decode1140(JObject jObject, JsonSerializer serializer)
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
				string format = versionToken.Value<string>();
				switch (format)
				{
					case "1.8.0":
					{
						foreach (var model in Decode180(jObject, serializer))
						{
							if (model.Bones != null)
							{
								foreach (var bone in model.Bones)
								{
									if (bone.Cubes != null)
									{
										foreach (var cube in bone.Cubes)
										{
											//cube.Rotation = new Vector3(90f, 0f, 0f);
										}
									}
								}
							}

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
							/*if (model.Bones != null)
							{
								foreach (var bone in model.Bones)
								{
									foreach (var cube in bone.Cubes)
									{
										
									}
								}
							}*/
							
							//TODO: Fix cube pivot Note that in 1.12 this is flipped upside-down, but is fixed in 1.14.

							result.TryAdd(model.Description.Identifier, model);
						}
						
						break;
					}

					case "1.14.0":
					{
						foreach (var model in Decode1140(jObject, serializer))
						{
							result.TryAdd(model.Description.Identifier, model);
						}
						break;
					}
					default:
						Log.Warn($"Invalid format_version: {format})");
						break;
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