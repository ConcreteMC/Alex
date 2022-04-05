using System;
using Alex.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class Vector2Converter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value is IVector2 ? (IVector2)value : Primitives.Factory.Vector2Zero;

			writer.WriteRawValue(JsonConvert.SerializeObject(new double[] { v.X, v.Y }, Formatting.None));
			/*
			serializer.Serialize(writer, new float[]
			{
				v.X,
				v.Y
			});*/
		}

		public override object ReadJson(JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.Array)
			{
				var arr = (JArray)obj;

				if (arr.Count == 2)
				{
					var x = 0f;
					var y = 0f;
					
					if (arr[0].Type == JTokenType.Integer)
					{
						x = arr[0].Value<int>();
					}
					else if (arr[0].Type == JTokenType.Float)
					{
						x = arr[0].Value<float>();
					}

					if (arr[1].Type == JTokenType.Integer)
					{
						y = arr[1].Value<int>();
					}
					else if (arr[1].Type == JTokenType.Float)
					{
						 y = arr[1].Value<float>();
					}

					return Primitives.Factory.Vector2(x, y);
				}
			}

			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(IVector2).IsAssignableFrom(objectType);
			;
		}
	}
}