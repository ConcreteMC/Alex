using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.BlockStates
{
	[JsonConverter(typeof(BlockStateVariantConverter))]
	public class BlockStateVariant : List<BlockStateModel>
	{
		public BlockStateVariant() : base() { }

		public BlockStateVariant(params BlockStateModel[] values) : base(values) { }
	}
}