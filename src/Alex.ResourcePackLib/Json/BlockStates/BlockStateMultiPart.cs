using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.BlockStates
{
	public class BlockStateMultiPart
	{
		[JsonConverter(typeof(SingleOrArrayConverter<BlockStateModel>))]
		public BlockStateModel[] Apply = null;

		[JsonConverter(typeof(SingleOrArrayConverter<MultiPartRule>))]
		public MultiPartRule[] When = null;
	}
}