using System;
using System.Linq;
using Newtonsoft.Json;
using ResourcePackLib.CoreRT.Json.BlockStates;

namespace ResourcePackLib.CoreRT.Json.Converters
{
	public class BlockStateVariantConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value as BlockStateVariant;

			if (v.Count > 1)
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
				var v = new BlockStateVariant(serializer.Deserialize<BlockStateModel[]>(reader));
				return v;
			}
			else
			{
				var v = new BlockStateVariant(serializer.Deserialize<BlockStateModel>(reader));
				return v;
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(BlockStateVariant).IsAssignableFrom(objectType);
		}
	}
}
