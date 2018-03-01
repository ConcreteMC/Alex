using Newtonsoft.Json;
using ResourcePackLib.CoreRT.Json.Converters;

namespace ResourcePackLib.CoreRT.Json.BlockStates
{
	public class BlockStateMultiPart
	{
		//[JsonProperty("apply")]
		//public string ApplyModel;

		//[JsonIgnore]
		[JsonConverter(typeof(BlockStateMultipartConverter))]
		public BlockStateModel[] Apply = null;

		[JsonConverter(typeof(BlockStateMultipartRuleConverter))]
		public MultiPartRule[] When = null;
	}

	public class PartCondtion
	{

	}
}
