using System;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Sound
{
	internal class SoundElementConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(SoundElement) || t == typeof(SoundElement?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.String:
				case JsonToken.Date:
					var stringValue = serializer.Deserialize<string>(reader);
					return new SoundElement { Path = stringValue };
				case JsonToken.StartObject:
					var objectValue = serializer.Deserialize<SoundMetadata>(reader);
					return new SoundElement { SoundMetadata = objectValue };
			}
			throw new Exception("Cannot unmarshal type SoundElement");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			var value = (SoundElement)untypedValue;
			if (value.Path != null)
			{
				serializer.Serialize(writer, value.Path);
				return;
			}
			if (value.SoundMetadata != null)
			{
				serializer.Serialize(writer, value.SoundMetadata);
				return;
			}
			throw new Exception("Cannot marshal type SoundElement");
		}

		public static readonly SoundElementConverter Singleton = new SoundElementConverter();
	}
}