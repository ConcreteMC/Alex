using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.ResourcePackLib.Json.Converters
{
	[JsonConverter(typeof(MobsModelConverter))]
	public class MobsModelDefinition : Dictionary<string, EntityModel>
	{
		
	}

	public enum FormatVersion
	{
		[JsonEnumValue("1.8.0")] V1_8_0,
		[JsonEnumValue("1.10.0")] V1_10_0,
		[JsonEnumValue("1.12.0")] V1_12_0,
		[JsonEnumValue("1.14.0")] V1_14_0,
		[JsonEnumValue("1.16.0")] V1_16_0,
		Unknown
	}

	public static class FormatVersionHelpers
	{
		private static IReadOnlyDictionary<string, FormatVersion> _helper;
		static FormatVersionHelpers()
		{
			Dictionary<string, FormatVersion> versions = new Dictionary<string, FormatVersion>();

			foreach (FieldInfo fi in typeof(FormatVersion).GetFields())
			{
				JsonEnumValueAttribute valueAttribute = fi.GetCustomAttribute<JsonEnumValueAttribute>();

				if (valueAttribute != null)
				{
					versions.Add(valueAttribute.Value, (FormatVersion)fi.GetValue(null));
				}
			}
			
			_helper = versions;
		}
		
		public static FormatVersion FromString(string value)
		{
			if (_helper.TryGetValue(value, out FormatVersion v))
				return v;
			
			return FormatVersion.Unknown;
		}
	}
	
	internal class MobsModelConverter : JsonConverter
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(MobsModelConverter));
		
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		private EntityModel DecodeSingle(JObject jObject, JsonSerializer serializer, FormatVersion version)
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
							/*if (version == FormatVersion.V1_8_0)
							{
								var bones = property.ToObject<EntityModelBoneV18[]>(serializer);

								if (bones != null)
								{
									model.Bones = bones.Select(x => x.Clone()).ToArray();
								}
							}
							else
							{*/
								model.Bones = property.ToObject<EntityModelBone[]>(serializer);
							//}

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
			
			if (model.Bones != null)
			{
				foreach (var bone in model.Bones)
				{
					if (bone.Pivot.HasValue)
					{
						bone.Pivot *= new Vector3(-1f, 1f, 1f);
					}
								
					//bone.BindPoseRotation
					if (bone.Rotation.HasValue)
					{
						bone.Rotation *= new Vector3(-1f, -1f, 1f);
					}
					
					if (bone.BindPoseRotation.HasValue)
					{
						bone.BindPoseRotation *= new Vector3(-1f, -1f, 1f);
					}
								
					if (bone.Cubes != null)
					{
						foreach (var cube in bone.Cubes)
						{
							var original = cube.Origin;
							
							cube.Origin = new Vector3(-(original.X + cube.Size.X), original.Y, original.Z);
							if (cube.Pivot.HasValue)
							{
								cube.Pivot *= new Vector3(-1f, 1f, 1f);
							}

							if (cube.Rotation.HasValue)
							{
								cube.Rotation *= new Vector3(-1f, -1f, 1f);
							}
						}
					}
				}
			}

			return model;
		}

		private IEnumerable<EntityModel> DecodeGeneric(JObject jObject, JsonSerializer serializer, FormatVersion formatVersion)
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
								yield return DecodeSingle((JObject) geo, serializer, formatVersion);
							}
						}
					}
					else if (property.Type == JTokenType.Object)
					{
						yield return DecodeSingle((JObject) property, serializer, formatVersion);
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


				if (property.Type == JTokenType.Array)
				{
					foreach (var geo in property.Values())
					{
						if (geo.Type == JTokenType.Object)
						{
							var singleDecode = DecodeSingle((JObject) geo, serializer, FormatVersion.V1_8_0);

							if (singleDecode != null)
							{
								singleDecode.Description.Identifier = prop.Key;

								yield return singleDecode;
							}
						}
					}
				}
				else if (property.Type == JTokenType.Object)
				{
					var singleDecode = DecodeSingle((JObject) property, serializer, FormatVersion.V1_8_0);

					if (singleDecode != null)
					{
						singleDecode.Description.Identifier = prop.Key;

						yield return singleDecode;
					}
				}
			}

			//return model;
		}
		
		private IEnumerable<EntityModel> Decode1100(JObject jObject, JsonSerializer serializer)
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


				if (property.Type == JTokenType.Array)
				{
					foreach (var geo in property.Values())
					{
						if (geo.Type == JTokenType.Object)
						{
							var singleDecode = DecodeSingle((JObject) geo, serializer, FormatVersion.V1_10_0);

							if (singleDecode != null)
							{
								singleDecode.Description.Identifier = prop.Key;

								yield return singleDecode;
							}
						}
					}
				}
				else if (property.Type == JTokenType.Object)
				{
					var singleDecode = DecodeSingle((JObject) property, serializer, FormatVersion.V1_10_0);

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

			string              formatVersion = "1.8.0";
			var                 jObject       = (JObject)obj;
			MobsModelDefinition result        = new MobsModelDefinition();
			if (jObject.TryGetValue(
				"format_version", StringComparison.InvariantCultureIgnoreCase, out var versionToken))
			{
				string format = versionToken.Value<string>();
				formatVersion = format;
			}

			switch (formatVersion)
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

				case "1.10.0":
				{
					foreach (var model in Decode1100(jObject, serializer))
					{
						result.TryAdd(model.Description.Identifier, model);
					}

					break;
				}

				case "1.12.0":
				{
					foreach (var model in DecodeGeneric(jObject, serializer,  FormatVersion.V1_12_0))
					{
						//TODO: Fix cube pivot Note that in 1.12 this is flipped upside-down, but is fixed in 1.14.

						result.TryAdd(model.Description.Identifier, model);
					}
						
					break;
				}

				case "1.14.0":
				{
					foreach (var model in DecodeGeneric(jObject, serializer, FormatVersion.V1_14_0))
					{
						result.TryAdd(model.Description.Identifier, model);
					}
					break;
				}
				
				case "1.16.0":
				{
					foreach (var model in DecodeGeneric(jObject, serializer, FormatVersion.V1_16_0))
					{
						result.TryAdd(model.Description.Identifier, model);
					}
					break;
				}
				
				default:
					Log.Warn($"Invalid format_version: {formatVersion})");
					break;
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