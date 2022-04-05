using System;
using System.Collections.Generic;
using Alex.Interfaces;
using Alex.ResourcePackLib.Json.Models.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class UvConverter : JsonConverter<EntityModelUV>
	{
		/// <inheritdoc />
		public override bool CanWrite => true;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, EntityModelUV value, JsonSerializer serializer)
		{
			var val = value;

			if (val.IsCube)
			{
				var v = val.Down.Origin;

				writer.WriteRawValue(JsonConvert.SerializeObject(new double[] { v.X, v.Y }, Formatting.None));

				return;
			}

			Dictionary<string, EntityModelUVData> newObject = new Dictionary<string, EntityModelUVData>();
			//JObject newObject = new JObject();
			newObject.Add("north", val.North);
			newObject.Add("east", val.East);
			newObject.Add("south", val.South);
			newObject.Add("west", val.West);
			newObject.Add("up", val.Up);
			newObject.Add("down", val.Down);

			serializer.Serialize(writer, newObject);
		}

		/// <inheritdoc />
		public override EntityModelUV ReadJson(JsonReader reader,
			Type objectType,
			EntityModelUV existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.Array)
			{
				var origin = ((JArray)(obj)).ToObject<IVector2>(serializer);

				return new EntityModelUV(origin);
			}

			var jObject = (JObject)obj;
			EntityModelUV uvData = new EntityModelUV();

			foreach (var property in jObject)
			{
				if (property.Value.Type != JTokenType.Object)
					continue;

				var j2 = (JObject)property.Value;

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
		}
	}
}