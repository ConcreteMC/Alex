using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.Blocks.Mapping
{
	[JsonConverter(typeof(BlockMapConverter))]
	public class BlockMap : Dictionary<string, BlockMappingEntry> { }

	public class BlockMappingEntry
	{
		[JsonProperty("bedrock_identifier")] public string BedrockIdentifier { get; set; }

		[JsonProperty("block_hardnessblock_hardness")]
		public float BlockHardness { get; set; } = 0.6f;

		[JsonProperty("can_break_with_hand")] public bool CanBreakWithHand { get; set; } = true;

		[JsonProperty("bedrock_states")] public Dictionary<string, string> BedrockStates { get; set; }
	}

	public class BlockMapConverter : JsonConverter
	{
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		private static readonly Regex Regex = new Regex(
			@"(?'key'[\:a-zA-Z_\d][^\[]*)(\[(?'data'.*)\])?", RegexOptions.Compiled);

		/// <inheritdoc />
		public override object? ReadJson(JsonReader reader,
			Type objectType,
			object? existingValue,
			JsonSerializer serializer)
		{
			BlockMap result = new BlockMap();

			var ob = JToken.Load(reader);

			if (ob is JObject obj)
			{
				foreach (var item in obj)
				{
					if (item.Value == null)
						continue;

					var key = item.Key;

					if (result.ContainsKey(key))
						continue;

					BlockMappingEntry a = new BlockMappingEntry();
					a.BedrockStates = new Dictionary<string, string>();

					JObject itemValue = (JObject)item.Value;

					foreach (var itemKey in itemValue)
					{
						if (itemKey.Key == "bedrock_identifier")
						{
							a.BedrockIdentifier = itemKey.Value.ToObject<string>();
						}
						else if (itemKey.Key == "bedrock_states")
						{
							if (itemKey.Value.Type == JTokenType.Object)
							{
								foreach (var state in (JObject)itemKey.Value)
								{
									switch (state.Value.Type)
									{
										case JTokenType.Boolean:
											a.BedrockStates.TryAdd(state.Key, state.Value.ToObject<bool>() ? "1" : "0");

											break;

										default:
											a.BedrockStates.TryAdd(state.Key, state.Value.ToObject<string>());

											break;
									}
								}
							}
						}
					}

					//var a     = item.Value.ToObject<BlockMappingEntry>(serializer);
					//var match = Regex.Match(key);

					result.Add(key, a);
				}
			}

			return result;
		}

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(BlockMap).IsAssignableFrom(objectType);
		}
	}
}