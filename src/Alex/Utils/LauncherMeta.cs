using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using J = Newtonsoft.Json.JsonPropertyAttribute;

namespace Alex
{
	public partial class LauncherMeta
	{
		[J("arguments")] public Arguments Arguments { get; set; }
		[J("assetIndex")] public LauncherMetaAssetIndex LauncherMetaAssetIndex { get; set; }
		[J("assets")] public string Assets { get; set; }
		[J("downloads")] public LauncherMetaDownloads Downloads { get; set; }
		[J("id")] public string Id { get; set; }
		[J("libraries")] public Library[] Libraries { get; set; }
		[J("logging")] public Logging Logging { get; set; }
		[J("mainClass")] public string MainClass { get; set; }
		[J("minimumLauncherVersion")] public long MinimumLauncherVersion { get; set; }
		[J("releaseTime")] public System.DateTimeOffset ReleaseTime { get; set; }
		[J("time")] public System.DateTimeOffset Time { get; set; }
		[J("type")] public string Type { get; set; }
	}

	public partial class Arguments
	{
		[J("game")] public GameElement[] Game { get; set; }
		[J("jvm")] public JvmElement[] Jvm { get; set; }
	}

	public partial class GameClass
	{
		[J("rules")] public GameRule[] Rules { get; set; }
		[J("value")] public Value Value { get; set; }
	}

	public partial class GameRule
	{
		[J("action")] public string Action { get; set; }
		[J("features")] public Features Features { get; set; }
	}

	public partial class Features
	{
		[J("is_demo_user")] public bool? IsDemoUser { get; set; }
		[J("has_custom_resolution")] public bool? HasCustomResolution { get; set; }
	}

	public partial class JvmClass
	{
		[J("rules")] public JvmRule[] Rules { get; set; }
		[J("value")] public Value Value { get; set; }
	}

	public partial class JvmRule
	{
		[J("action")] public string Action { get; set; }
		[J("os")] public PurpleOs Os { get; set; }
	}

	public partial class PurpleOs
	{
		[J("name")] public string Name { get; set; }
		[J("version")] public string Version { get; set; }
	}

	public partial class LauncherMetaAssetIndex
	{
		[J("id")] public string Id { get; set; }
		[J("sha1")] public string Sha1 { get; set; }
		[J("size")] public long Size { get; set; }
		[J("url")] public string Url { get; set; }
		[J("totalSize")] public long? TotalSize { get; set; }
	}

	public partial class LauncherMetaDownloads
	{
		[J("client")] public DownloadsClient Client { get; set; }
		[J("server")] public DownloadsClient Server { get; set; }
	}

	public partial class DownloadsClient
	{
		[J("sha1")] public string Sha1 { get; set; }
		[J("size")] public long Size { get; set; }
		[J("url")] public string Url { get; set; }
		[J("path")] public string Path { get; set; }
	}

	public partial class Library
	{
		[J("name")] public string Name { get; set; }
		[J("downloads")] public LibraryDownloads Downloads { get; set; }
		[J("natives")] public Natives Natives { get; set; }
		[J("extract")] public Extract Extract { get; set; }
		[J("rules")] public LibraryRule[] Rules { get; set; }
	}

	public partial class LibraryDownloads
	{
		[J("artifact")] public DownloadsClient Artifact { get; set; }
		[J("classifiers")] public Classifiers Classifiers { get; set; }
	}

	public partial class Classifiers
	{
		[J("tests")] public DownloadsClient Tests { get; set; }
		[J("natives-linux")] public DownloadsClient NativesLinux { get; set; }
		[J("natives-macos")] public DownloadsClient NativesMacos { get; set; }
		[J("natives-windows")] public DownloadsClient NativesWindows { get; set; }
		[J("natives-osx")] public DownloadsClient NativesOsx { get; set; }
	}

	public partial class Extract
	{
		[J("exclude")] public string[] Exclude { get; set; }
	}

	public partial class Natives
	{
		[J("linux")] public string Linux { get; set; }
		[J("osx")] public string Osx { get; set; }
		[J("windows")] public string Windows { get; set; }
	}

	public partial class LibraryRule
	{
		[J("action")] public string Action { get; set; }
		[J("os")] public FluffyOs Os { get; set; }
	}

	public partial class FluffyOs
	{
		[J("name")] public string Name { get; set; }
	}

	public partial class Logging
	{
		[J("client")] public LoggingClient Client { get; set; }
	}

	public partial class LoggingClient
	{
		[J("file")] public LauncherMetaAssetIndex File { get; set; }
		[J("argument")] public string Argument { get; set; }
		[J("type")] public string Type { get; set; }
	}

	public partial struct GameElement
	{
		public GameClass GameClass;
		public string String;
	}

	public partial struct Value
	{
		public string String;
		public string[] StringArray;
	}

	public partial struct JvmElement
	{
		public JvmClass JvmClass;
		public string String;
	}

	public partial class LauncherMeta
	{
		public static LauncherMeta FromJson(string json) => JsonConvert.DeserializeObject<LauncherMeta>(json, Converter.Settings);
	}

	public partial struct GameElement
	{
		public GameElement(JsonReader reader, JsonSerializer serializer)
		{
			GameClass = null;
			String = null;

			switch (reader.TokenType)
			{
				case JsonToken.StartObject:
					GameClass = serializer.Deserialize<GameClass>(reader);
					return;
				case JsonToken.String:
				case JsonToken.Date:
					String = serializer.Deserialize<string>(reader);
					return;
			}
			throw new Exception("Cannot convert GameElement");
		}

		public void WriteJson(JsonWriter writer, JsonSerializer serializer)
		{
			if (GameClass != null)
			{
				serializer.Serialize(writer, GameClass);
				return;
			}
			if (String != null)
			{
				serializer.Serialize(writer, String);
				return;
			}
			throw new Exception("Union must not be null");
		}
	}

	public partial struct Value
	{
		public Value(JsonReader reader, JsonSerializer serializer)
		{
			String = null;
			StringArray = null;

			switch (reader.TokenType)
			{
				case JsonToken.StartArray:
					StringArray = serializer.Deserialize<string[]>(reader);
					return;
				case JsonToken.String:
				case JsonToken.Date:
					String = serializer.Deserialize<string>(reader);
					return;
			}
			throw new Exception("Cannot convert Value");
		}

		public void WriteJson(JsonWriter writer, JsonSerializer serializer)
		{
			if (String != null)
			{
				serializer.Serialize(writer, String);
				return;
			}
			if (StringArray != null)
			{
				serializer.Serialize(writer, StringArray);
				return;
			}
			throw new Exception("Union must not be null");
		}
	}

	public partial struct JvmElement
	{
		public JvmElement(JsonReader reader, JsonSerializer serializer)
		{
			JvmClass = null;
			String = null;

			switch (reader.TokenType)
			{
				case JsonToken.StartObject:
					JvmClass = serializer.Deserialize<JvmClass>(reader);
					return;
				case JsonToken.String:
				case JsonToken.Date:
					String = serializer.Deserialize<string>(reader);
					return;
			}
			throw new Exception("Cannot convert JvmElement");
		}

		public void WriteJson(JsonWriter writer, JsonSerializer serializer)
		{
			if (JvmClass != null)
			{
				serializer.Serialize(writer, JvmClass);
				return;
			}
			if (String != null)
			{
				serializer.Serialize(writer, String);
				return;
			}
			throw new Exception("Union must not be null");
		}
	}

	public static class Serialize
	{
		public static string ToJson(this LauncherMeta self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}

	internal class Converter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(GameElement) || t == typeof(Value) || t == typeof(JvmElement);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (t == typeof(GameElement))
				return new GameElement(reader, serializer);
			if (t == typeof(Value))
				return new Value(reader, serializer);
			if (t == typeof(JvmElement))
				return new JvmElement(reader, serializer);
			throw new Exception("Unknown type");
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var t = value.GetType();
			if (t == typeof(GameElement))
			{
				((GameElement)value).WriteJson(writer, serializer);
				return;
			}
			if (t == typeof(Value))
			{
				((Value)value).WriteJson(writer, serializer);
				return;
			}
			if (t == typeof(JvmElement))
			{
				((JvmElement)value).WriteJson(writer, serializer);
				return;
			}
			throw new Exception("Unknown type");
		}

		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters = {
				new Converter(),
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};
	}
}
