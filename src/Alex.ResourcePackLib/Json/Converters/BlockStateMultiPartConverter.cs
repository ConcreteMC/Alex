using System;
using System.Linq;
using Alex.ResourcePackLib.Json.BlockStates;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class BlockStateMultipartConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value as BlockStateModel[];

			if (v.Length > 1)
			{
				serializer.Serialize(writer, v.ToArray());
			}
			else
			{
				serializer.Serialize(writer, v.FirstOrDefault());
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.StartArray)
			{
				var v = serializer.Deserialize<BlockStateModel[]>(reader);
				return v;
			}
			else
			{
				var v = new BlockStateModel[] {serializer.Deserialize<BlockStateModel>(reader)};
				return v;
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(BlockStateModel).IsAssignableFrom(objectType);
		}
	}

	public class BlockStateMultipartRuleConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value as MultiPartRule[];

			if (v.Length > 1)
			{
				serializer.Serialize(writer, v.ToArray());
			}
			else
			{
				serializer.Serialize(writer, v.FirstOrDefault());
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.StartArray)
			{
				var v = serializer.Deserialize<MultiPartRule[]>(reader);
				return v;
			}
			else
			{
				var v = new MultiPartRule[] { serializer.Deserialize<MultiPartRule>(reader) };
				return v;
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(MultiPartRule).IsAssignableFrom(objectType);
		}
	}
}
