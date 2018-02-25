using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ResourcePackLib.Json.Converters;

namespace ResourcePackLib.Json.BlockStates
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
