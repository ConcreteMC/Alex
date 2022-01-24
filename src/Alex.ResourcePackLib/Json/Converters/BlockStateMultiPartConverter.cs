using System;
using System.Linq;
using Alex.ResourcePackLib.Json.BlockStates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

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

		public override object ReadJson(JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.StartArray)
			{
				var v = serializer.Deserialize<BlockStateModel[]>(reader);

				return v;
			}
			else
			{
				var v = new BlockStateModel[] { serializer.Deserialize<BlockStateModel>(reader) };

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
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

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

		public override object ReadJson(JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer)
		{
			MultiPartRule rule = new MultiPartRule();

			JObject obj = JObject.Load(reader);

			foreach (var prop in obj)
			{
				switch (prop.Key.ToLower())
				{
					case "or":
						if (prop.Value.Type == JTokenType.Array)
						{
							rule.Or = prop.Value.ToObject<MultiPartRule[]>(serializer);
						}
						else
						{
							rule.Or = new MultiPartRule[] { prop.Value.ToObject<MultiPartRule>(serializer) };
						}

						break;

					case "and":
						if (prop.Value.Type == JTokenType.Array)
						{
							rule.And = prop.Value.ToObject<MultiPartRule[]>(serializer);
						}
						else
						{
							rule.And = new MultiPartRule[] { prop.Value.ToObject<MultiPartRule>(serializer) };
						}

						break;

					default:
						if (prop.Value.Type == JTokenType.String || prop.Value.Type == JTokenType.Boolean)
						{
							rule.KeyValues.Add(prop.Key, prop.Value.ToObject<string>());
						}
						else
						{
							Log.Warn($"Got unsupported property type: {prop.Key}:{prop.Value.Type}");
						}

						break;
				}
			}


			/*if (reader.TokenType == JsonToken.StartArray)
			{
				var v = serializer.Deserialize<MultiPartRule[]>(reader);
				return v;
			}
			else
			{
				var v = new MultiPartRule[] { serializer.Deserialize<MultiPartRule>(reader) };
				return v;
			}*/

			return rule;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(MultiPartRule).IsAssignableFrom(objectType);
		}
	}
}