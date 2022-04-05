using System;
using System.Linq;
using Alex.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class Vector3Converter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value is IVector3 ? (IVector3)value : Primitives.Factory.Vector3Zero;

			writer.WriteRawValue(JsonConvert.SerializeObject(new float[] { v.X, v.Y, v.Z }, Formatting.None));
			/*serializer.Serialize(writer, new float[]
			{
				v.X,
				v.Y,
				v.Z
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

				if (arr.Count == 3)
				{
					var x = 0f;
					var y = 0f;
					var z = 0f;
					var v3 = Primitives.Factory.Vector3(0,0,0);

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

					if (arr[2].Type == JTokenType.Integer)
					{
						z = arr[2].Value<int>();
					}
					else if (arr[2].Type == JTokenType.Float)
					{
						z = arr[2].Value<float>();
					}

					return Primitives.Factory.Vector3(x,y,z);
				}
			}

			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(IVector3).IsAssignableFrom(objectType) || typeof(IVector3).IsAssignableFrom(objectType);
		}
	}
}