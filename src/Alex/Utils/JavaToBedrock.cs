using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Alex.Utils
{
	public class MapItem
	{
		[JsonProperty("bedrock_identifier")]
		public string BedrockIdentifier { get; set; }

		[JsonProperty("block_hardness")]
		public double BlockHardness { get; set; }

		[JsonProperty("can_break_with_hand")]
		public bool CanBreakWithHand { get; set; }

		[JsonProperty("tool_type")]
		public string ToolType { get; set; }

		[JsonProperty("bedrock_states")]
		public ReadOnlyDictionary<string, string> BedrockStates { get; set; }
	}
}