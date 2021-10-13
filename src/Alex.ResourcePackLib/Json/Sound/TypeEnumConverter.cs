using System;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Sound
{
	internal class TypeEnumConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(SoundType) || t == typeof(SoundType?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;
			var value = serializer.Deserialize<string>(reader);
			if (value == "event")
			{
				return SoundType.Event;
			}
			throw new Exception("Cannot unmarshal type TypeEnum");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			if (untypedValue == null)
			{
				serializer.Serialize(writer, null);
				return;
			}
			var value = (SoundType)untypedValue;
			if (value == SoundType.Event)
			{
				serializer.Serialize(writer, "event");
				return;
			}
			throw new Exception("Cannot marshal type TypeEnum");
		}

		public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
	}
}