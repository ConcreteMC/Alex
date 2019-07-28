using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{

	public class Vector3Converter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value is Vector3 ? (Vector3) value : new Vector3();

			serializer.Serialize(writer, new float[]
			{
				v.X,
				v.Y,
				v.Z
			});
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.Array)
			{
				var arr = (JArray)obj;
				if (arr.Count == 3)
				{
					var v3 = new Vector3();

					if (arr[0].Type == JTokenType.Integer)
					{
						v3.X = arr[0].Value<int>();
					}
					else if (arr[0].Type == JTokenType.Float)
					{
						v3.X = arr[0].Value<float>();
					}
					
					if (arr[1].Type == JTokenType.Integer)
					{
						v3.Y = arr[1].Value<int>();
					}
					else if (arr[1].Type == JTokenType.Float)
					{
						v3.Y = arr[1].Value<float>();
					}

					if (arr[2].Type == JTokenType.Integer)
					{
						v3.Z = arr[2].Value<int>();
					}
					else if (arr[2].Type == JTokenType.Float)
					{
						v3.Z = arr[2].Value<float>();
					}

					return v3;
				}
			}

			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(Vector3).IsAssignableFrom(objectType);
		}
	}

	public class Vector2Converter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value is Vector2 ? (Vector2) value : new Vector2();

			serializer.Serialize(writer, new float[]
			{
				v.X,
				v.Y
			});
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.Array)
			{
				var arr = (JArray)obj;
				if (arr.Count == 2)
				{
					var v3 = new Vector2();

					if (arr[0].Type == JTokenType.Integer)
					{
						v3.X = arr[0].Value<int>();
					}
					else if (arr[0].Type == JTokenType.Float)
					{
						v3.X = arr[0].Value<float>();
					}
					
					if (arr[1].Type == JTokenType.Integer)
					{
						v3.Y = arr[1].Value<int>();
					}
					else if (arr[1].Type == JTokenType.Float)
					{
						v3.Y = arr[1].Value<float>();
					}

					return v3;
				}
			}

			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(Vector2).IsAssignableFrom(objectType);
		}
}
}
