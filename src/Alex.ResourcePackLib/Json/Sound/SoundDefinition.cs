using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.ResourcePackLib.Json.Sound
{
    public partial class SoundDefinition
    {
        [JsonProperty("sounds")]
        public SoundElement[] Sounds { get; set; }

        [JsonProperty("subtitle", NullValueHandling = NullValueHandling.Ignore)]
        public string Subtitle { get; set; }
        
        public static Dictionary<string, SoundDefinition> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<string, SoundDefinition>>(json, Converter.Settings);
    }

    public partial class SoundClass
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("stream", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Stream { get; set; }

        [JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
        public double? Volume { get; set; }

        [JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
        public long? Weight { get; set; }

        [JsonProperty("pitch", NullValueHandling = NullValueHandling.Ignore)]
        public double? Pitch { get; set; }

        [JsonProperty("attenuation_distance", NullValueHandling = NullValueHandling.Ignore)]
        public long? AttenuationDistance { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public TypeEnum? Type { get; set; }
    }

    public enum TypeEnum { Event };

    public partial struct SoundElement
    {
        public SoundClass SoundClass;
        public string Path;

        public static implicit operator SoundElement(SoundClass soundClass) => new SoundElement { SoundClass = soundClass };
        public static implicit operator SoundElement(string path) => new SoundElement { Path = path };
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                SoundElementConverter.Singleton,
                TypeEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

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
                    var objectValue = serializer.Deserialize<SoundClass>(reader);
                    return new SoundElement { SoundClass = objectValue };
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
            if (value.SoundClass != null)
            {
                serializer.Serialize(writer, value.SoundClass);
                return;
            }
            throw new Exception("Cannot marshal type SoundElement");
        }

        public static readonly SoundElementConverter Singleton = new SoundElementConverter();
    }

    internal class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "event")
            {
                return TypeEnum.Event;
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
            var value = (TypeEnum)untypedValue;
            if (value == TypeEnum.Event)
            {
                serializer.Serialize(writer, "event");
                return;
            }
            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }
}
