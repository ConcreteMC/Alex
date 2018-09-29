using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace Alex
{
	public partial class VersionManifest
	{
		[J("latest")] public Latest Latest { get; set; }
		[J("versions")] public Version[] Versions { get; set; }
	}

	public partial class Latest
	{
		[J("snapshot")] public string Snapshot { get; set; }
		[J("release")] public string Release { get; set; }
	}

	public partial class Version
	{
		[J("id")] public string Id { get; set; }
		[J("type")] public VersionType Type { get; set; }
		[J("time")] public System.DateTimeOffset Time { get; set; }
		[J("releaseTime")] public System.DateTimeOffset ReleaseTime { get; set; }
		[J("url")] public string Url { get; set; }
	}

	public enum VersionType { OldAlpha, OldBeta, Release, Snapshot };

	public partial class VersionManifest
	{
		public static VersionManifest FromJson(string json) => JsonConvert.DeserializeObject<VersionManifest>(json, VersionManifestConverter.Settings);
	}

	static class VersionTypeExtensions
	{
		public static VersionType? ValueForString(string str)
		{
			switch (str)
			{
				case "old_alpha": return VersionType.OldAlpha;
				case "old_beta": return VersionType.OldBeta;
				case "release": return VersionType.Release;
				case "snapshot": return VersionType.Snapshot;
				default: return null;
			}
		}

		public static VersionType ReadJson(JsonReader reader, JsonSerializer serializer)
		{
			var str = serializer.Deserialize<string>(reader);
			var maybeValue = ValueForString(str);
			if (maybeValue.HasValue) return maybeValue.Value;
			throw new Exception("Unknown enum case " + str);
		}

		public static void WriteJson(this VersionType value, JsonWriter writer, JsonSerializer serializer)
		{
			switch (value)
			{
				case VersionType.OldAlpha: serializer.Serialize(writer, "old_alpha"); break;
				case VersionType.OldBeta: serializer.Serialize(writer, "old_beta"); break;
				case VersionType.Release: serializer.Serialize(writer, "release"); break;
				case VersionType.Snapshot: serializer.Serialize(writer, "snapshot"); break;
			}
		}
	}

	public static class VersionManifestSerialize
	{
		public static string ToJson(this VersionManifest self) => JsonConvert.SerializeObject(self, VersionManifestConverter.Settings);
	}

	internal class VersionManifestConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(VersionType) || t == typeof(VersionType?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (t == typeof(VersionType))
				return VersionTypeExtensions.ReadJson(reader, serializer);
			if (t == typeof(VersionType?))
			{
				if (reader.TokenType == JsonToken.Null) return null;
				return VersionTypeExtensions.ReadJson(reader, serializer);
			}
			throw new Exception("Unknown type");
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var t = value.GetType();
			if (t == typeof(VersionType))
			{
				((VersionType)value).WriteJson(writer, serializer);
				return;
			}
			throw new Exception("Unknown type");
		}

		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters = {
				new VersionManifestConverter(),
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};
	}
}
