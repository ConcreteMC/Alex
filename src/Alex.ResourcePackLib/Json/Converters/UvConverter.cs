using System;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class UvConverter : JsonConverter
	{
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			var val = value as EntityModelUV;

			if (val.IsCube)
			{
				var v = val.Down.Origin;

				serializer.Serialize(writer, new float[]
				{
					v.X,
					v.Y
				});

				return;
			}
			
			serializer.Serialize(writer, val);
		}

		/// <inheritdoc />
		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var               obj     = JToken.Load(reader);

			if (obj.Type == JTokenType.Array)
			{
				var origin = ((JArray)(obj)).ToObject<Vector2>(serializer);

				return new EntityModelUV(origin);
			}
			
			var           jObject = (JObject)obj;
			EntityModelUV uvData  = new EntityModelUV();
			foreach (var property in jObject)
			{
				if (property.Value.Type != JTokenType.Object)
					continue;

				var j2 = (JObject) property.Value;
				switch (property.Key)
				{
					case "north":
						uvData.North = j2.ToObject<EntityModelUVData>(serializer);
						break;
					case "east":
						uvData.East = j2.ToObject<EntityModelUVData>(serializer);
						break;
					case "south":
						uvData.South = j2.ToObject<EntityModelUVData>(serializer);
						break;
					case "west":
						uvData.West = j2.ToObject<EntityModelUVData>(serializer);
						break;
					case "up":
						uvData.Up = j2.ToObject<EntityModelUVData>(serializer);
						break;
					case "down":
						uvData.Down = j2.ToObject<EntityModelUVData>(serializer);
						break;
				}
			}

			return uvData;
			//return jObject.ToObject<>()
		}

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(EntityModelUV).IsAssignableFrom(objectType);
		}
	}
}