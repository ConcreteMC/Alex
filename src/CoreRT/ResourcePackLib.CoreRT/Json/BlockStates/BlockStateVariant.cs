using System.Collections.Generic;
using Newtonsoft.Json;
using ResourcePackLib.CoreRT.Json.Converters;

namespace ResourcePackLib.CoreRT.Json.BlockStates
{
	[JsonConverter(typeof(BlockStateVariantConverter))]
	public class BlockStateVariant : List<BlockStateModel>
	{
		public BlockStateVariant() : base()
		{

		}

		public BlockStateVariant(params BlockStateModel[] values) : base(values)
		{

		}
	}
}
