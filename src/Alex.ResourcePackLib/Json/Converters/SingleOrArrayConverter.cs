using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class SingleOrArrayConverter<T> : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (!(value is List<T> list))
			{
				list = new List<T>();
			}

			if (list.Count == 1)
			{
				serializer.Serialize(writer, list.FirstOrDefault());
			}

			serializer.Serialize(writer, list);

		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken token = JToken.Load(reader);
			if (token.Type == JTokenType.Array)
			{
				return token.ToObject(objectType);
//				return token.ToObject<List<T>>();
			}

			var obj = token.ToObject<T>();

			if (objectType == typeof(T[]))
			{
				return new T[] {obj};
			}

			return new List<T> {token.ToObject<T>()};
		}

		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof(List<T>) || objectType == typeof(T[]));
		}
	}
}