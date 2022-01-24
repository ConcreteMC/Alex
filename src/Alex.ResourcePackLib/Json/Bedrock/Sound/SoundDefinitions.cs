using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Alex.ResourcePackLib.Json.Bedrock.Sound
{
	public class SoundDefinitionFormat
	{
		[JsonProperty("format_version")] public string FormatVersion { get; set; }

		[JsonProperty("sound_definitions")] public Dictionary<string, SoundDefinition> SoundDefinitions { get; set; }

		public static SoundDefinitionFormat FromJson(string json) =>
			JsonConvert.DeserializeObject<SoundDefinitionFormat>(json, Settings);

		private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters =
			{
				SoundElementConverter.Singleton,
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};
	}

	public class SoundDefinition
	{
		[JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
		public SoundCategory Category { get; set; }

		[JsonProperty("sounds")] public SoundElement[] Sounds { get; set; }

		[JsonProperty("__use_legacy_max_distance", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(ParseStringConverter))]
		public bool? UseLegacyMaxDistance { get; set; }

		[JsonProperty("min_distance", NullValueHandling = NullValueHandling.Ignore)]
		public float? MinDistance { get; set; }

		[JsonProperty("max_distance", NullValueHandling = NullValueHandling.Ignore)]
		public float? MaxDistance { get; set; }

		[JsonProperty("subtitle", NullValueHandling = NullValueHandling.Ignore)]
		public string Subtitle { get; set; }

		[JsonProperty("pitch", NullValueHandling = NullValueHandling.Ignore)]
		public float? Pitch { get; set; }
	}

	public class SoundClass
	{
		[JsonProperty("load_on_low_memory", NullValueHandling = NullValueHandling.Ignore)]
		public bool? LoadOnLowMemory { get; set; }

		[JsonProperty("name")] public string Name { get; set; }

		[JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
		public float? Volume { get; set; }

		[JsonProperty("pitch", NullValueHandling = NullValueHandling.Ignore)]
		public float? Pitch { get; set; }

		[JsonProperty("stream", NullValueHandling = NullValueHandling.Ignore)]
		public bool? Stream { get; set; }

		[JsonProperty("is3D", NullValueHandling = NullValueHandling.Ignore)]
		public bool? Is3D { get; set; } = true;

		[JsonProperty("weight", NullValueHandling = NullValueHandling.Ignore)]
		public int? Weight { get; set; } = 1;
	}

	public struct SoundElement
	{
		public SoundClass SoundClass;
		public string Path;

		public static implicit operator SoundElement(SoundClass soundClass) =>
			new SoundElement { SoundClass = soundClass };

		public static implicit operator SoundElement(string path) => new SoundElement { Path = path };
	}

	internal class ParseStringConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(bool) || t == typeof(bool?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return null;
			var value = serializer.Deserialize<string>(reader);
			bool b;

			if (Boolean.TryParse(value, out b))
			{
				return b;
			}

			throw new Exception("Cannot unmarshal type bool");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			if (untypedValue == null)
			{
				serializer.Serialize(writer, null);

				return;
			}

			var value = (bool)untypedValue;
			var boolString = value ? "true" : "false";
			serializer.Serialize(writer, boolString);

			return;
		}

		public static readonly ParseStringConverter Singleton = new ParseStringConverter();
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
}