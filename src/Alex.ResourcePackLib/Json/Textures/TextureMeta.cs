using System;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.ResourcePackLib.Json.Textures
{
    public class TextureMeta
    {
        [JsonProperty("animation")]
        public Animation Animation { get; set; }

        [JsonProperty("texture")]
        public Texture Texture { get; set; }
        
        public static TextureMeta FromJson(string json) => JsonConvert.DeserializeObject<TextureMeta>(json, Converter.Settings);
    }

    public class Animation
    {
        [JsonProperty("interpolate")]
        public bool Interpolate { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("frametime")] public int Frametime { get; set; } = 1;

        [JsonProperty("frames")]
        public FrameElement[] Frames { get; set; }
    }

    public class FrameClass
    {
        [JsonProperty("index")]
        public long Index { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }
    }

    public class Texture
    {
        [JsonProperty("blur")]
        public bool Blur { get; set; }

        [JsonProperty("clamp")]
        public bool Clamp { get; set; }
    }

    public struct FrameElement
    {
        public FrameClass FrameClass;
        public long? Integer;

        public static implicit operator FrameElement(FrameClass FrameClass) => new FrameElement { FrameClass = FrameClass };
        public static implicit operator FrameElement(long Integer) => new FrameElement { Integer = Integer };
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                FrameElementConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

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
                    var objectValue = serializer.Deserialize<FrameClass>(reader);
                    return new FrameElement { FrameClass = objectValue };
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
            if (value.FrameClass != null)
            {
                serializer.Serialize(writer, value.FrameClass);
                return;
            }
            throw new Exception("Cannot marshal type FrameElement");
        }

        public static readonly FrameElementConverter Singleton = new FrameElementConverter();
    }
}