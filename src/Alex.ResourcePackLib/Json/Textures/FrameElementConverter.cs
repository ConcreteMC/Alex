using System;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Textures
{
	internal class FrameElementConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(FrameElement) || t == typeof(FrameElement?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.Integer:
					var integerValue = serializer.Deserialize<long>(reader);

					return new FrameElement { Integer = integerValue };

				case JsonToken.StartObject:
					var objectValue = serializer.Deserialize<TextureFrame>(reader);

					return new FrameElement { FrameInfo = objectValue };
			}

			throw new Exception("Cannot unmarshal type FrameElement");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			var value = (FrameElement)untypedValue;

			if (value.Integer != null)
			{
				serializer.Serialize(writer, value.Integer.Value);

				return;
			}

			if (value.FrameInfo != null)
			{
				serializer.Serialize(writer, value.FrameInfo);

				return;
			}

			throw new Exception("Cannot marshal type FrameElement");
		}

		public static readonly FrameElementConverter Singleton = new FrameElementConverter();
	}
}