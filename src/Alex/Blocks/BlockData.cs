using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Blocks
{
	using System;
	using System.Collections.Generic;
	using System.Net;

	using System.Globalization;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;
	using J = Newtonsoft.Json.JsonPropertyAttribute;

	public partial class BlockData
	{
		[J("id")] public long Id { get; set; }
		[J("displayName")] public string DisplayName { get; set; }
		[J("name")] public string Name { get; set; }
		[J("hardness")] public double? Hardness { get; set; }
		[J("stackSize")] public long StackSize { get; set; }
		[J("diggable")] public bool Diggable { get; set; }
		[J("boundingBox")] public BoundingBox BoundingBox { get; set; }
		[J("drops")] public DropElement[] Drops { get; set; }
		[J("transparent")] public bool Transparent { get; set; }
		[J("emitLight")] public long EmitLight { get; set; }
		[J("filterLight")] public long FilterLight { get; set; }
		[J("material")] public BlockMaterial? Material { get; set; }
		[J("harvestTools")] public HarvestTools HarvestTools { get; set; }
		[J("variations")] public Variation[] Variations { get; set; }
	}

	public partial class DropElement
	{
		[J("drop")] public DropUnion Drop { get; set; }
		[J("minCount")] public double? MinCount { get; set; }
		[J("maxCount")] public long? MaxCount { get; set; }
	}

	public partial class DropDropClass
	{
		[J("id")] public long Id { get; set; }
		[J("metadata")] public long Metadata { get; set; }
	}

	public partial class HarvestTools
	{
		[J("257")] public bool? The257 { get; set; }
		[J("270")] public bool? The270 { get; set; }
		[J("274")] public bool? The274 { get; set; }
		[J("278")] public bool? The278 { get; set; }
		[J("285")] public bool? The285 { get; set; }
		[J("267")] public bool? The267 { get; set; }
		[J("268")] public bool? The268 { get; set; }
		[J("272")] public bool? The272 { get; set; }
		[J("276")] public bool? The276 { get; set; }
		[J("283")] public bool? The283 { get; set; }
		[J("359")] public bool? The359 { get; set; }
		[J("256")] public bool? The256 { get; set; }
		[J("269")] public bool? The269 { get; set; }
		[J("273")] public bool? The273 { get; set; }
		[J("277")] public bool? The277 { get; set; }
		[J("284")] public bool? The284 { get; set; }
	}

	public partial class Variation
	{
		[J("metadata")] public long Metadata { get; set; }
		[J("displayName")] public string DisplayName { get; set; }
	}

	public enum BoundingBox { Block, Empty };

	public enum BlockMaterial { Dirt, Plant, Rock, Web, Wood, Wool };

	public partial struct DropUnion
	{
		public DropDropClass DropDropClass;
		public long? Integer;
	}

	public partial class BlockData
	{
		public static BlockData[] FromJson(string json) => JsonConvert.DeserializeObject<BlockData[]>(json, Converter.Settings);
	}

	static class BoundingBoxExtensions
	{
		public static BoundingBox? ValueForString(string str)
		{
			switch (str)
			{
				case "block": return BoundingBox.Block;
				case "empty": return BoundingBox.Empty;
				default: return null;
			}
		}

		public static BoundingBox ReadJson(JsonReader reader, JsonSerializer serializer)
		{
			var str = serializer.Deserialize<string>(reader);
			var maybeValue = ValueForString(str);
			if (maybeValue.HasValue) return maybeValue.Value;
			throw new Exception("Unknown enum case " + str);
		}

		public static void WriteJson(this BoundingBox value, JsonWriter writer, JsonSerializer serializer)
		{
			switch (value)
			{
				case BoundingBox.Block: serializer.Serialize(writer, "block"); break;
				case BoundingBox.Empty: serializer.Serialize(writer, "empty"); break;
			}
		}
	}

	static class MaterialExtensions
	{
		public static BlockMaterial? ValueForString(string str)
		{
			switch (str)
			{
				case "dirt": return BlockMaterial.Dirt;
				case "plant": return BlockMaterial.Plant;
				case "rock": return BlockMaterial.Rock;
				case "web": return BlockMaterial.Web;
				case "wood": return BlockMaterial.Wood;
				case "wool": return BlockMaterial.Wool;
				default: return null;
			}
		}

		public static BlockMaterial ReadJson(JsonReader reader, JsonSerializer serializer)
		{
			var str = serializer.Deserialize<string>(reader);
			var maybeValue = ValueForString(str);
			if (maybeValue.HasValue) return maybeValue.Value;
			throw new Exception("Unknown enum case " + str);
		}

		public static void WriteJson(this BlockMaterial value, JsonWriter writer, JsonSerializer serializer)
		{
			switch (value)
			{
				case BlockMaterial.Dirt: serializer.Serialize(writer, "dirt"); break;
				case BlockMaterial.Plant: serializer.Serialize(writer, "plant"); break;
				case BlockMaterial.Rock: serializer.Serialize(writer, "rock"); break;
				case BlockMaterial.Web: serializer.Serialize(writer, "web"); break;
				case BlockMaterial.Wood: serializer.Serialize(writer, "wood"); break;
				case BlockMaterial.Wool: serializer.Serialize(writer, "wool"); break;
			}
		}
	}

	public partial struct DropUnion
	{
		public DropUnion(JsonReader reader, JsonSerializer serializer)
		{
			DropDropClass = null;
			Integer = null;

			switch (reader.TokenType)
			{
				case JsonToken.Integer:
					Integer = serializer.Deserialize<long>(reader);
					return;
				case JsonToken.StartObject:
					DropDropClass = serializer.Deserialize<DropDropClass>(reader);
					return;
			}
			throw new Exception("Cannot convert DropUnion");
		}

		public void WriteJson(JsonWriter writer, JsonSerializer serializer)
		{
			if (DropDropClass != null)
			{
				serializer.Serialize(writer, DropDropClass);
				return;
			}
			if (Integer != null)
			{
				serializer.Serialize(writer, Integer);
				return;
			}
			throw new Exception("Union must not be null");
		}
	}

	public static class Serialize
	{
		public static string ToJson(this BlockData[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
	}

	internal class Converter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(BoundingBox) || t == typeof(BlockMaterial) || t == typeof(DropUnion) || t == typeof(BoundingBox?) || t == typeof(BlockMaterial?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (t == typeof(BoundingBox))
				return BoundingBoxExtensions.ReadJson(reader, serializer);
			if (t == typeof(BlockMaterial))
				return MaterialExtensions.ReadJson(reader, serializer);
			if (t == typeof(BoundingBox?))
			{
				if (reader.TokenType == JsonToken.Null) return null;
				return BoundingBoxExtensions.ReadJson(reader, serializer);
			}
			if (t == typeof(BlockMaterial?))
			{
				if (reader.TokenType == JsonToken.Null) return null;
				return MaterialExtensions.ReadJson(reader, serializer);
			}
			if (t == typeof(DropUnion))
				return new DropUnion(reader, serializer);
			throw new Exception("Unknown type");
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var t = value.GetType();
			if (t == typeof(BoundingBox))
			{
				((BoundingBox)value).WriteJson(writer, serializer);
				return;
			}
			if (t == typeof(BlockMaterial))
			{
				((BlockMaterial)value).WriteJson(writer, serializer);
				return;
			}
			if (t == typeof(DropUnion))
			{
				((DropUnion)value).WriteJson(writer, serializer);
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
