using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ResourcePackLib.Json.BlockStates
{
	public class BlockState
	{
		/// <summary>
		/// Holds the names of all the variants of the block.
		/// </summary>
		public Dictionary<string, BlockStateVariant> Variants { get; set; } = new Dictionary<string, BlockStateVariant>();

		[JsonProperty("multipart")]
		public BlockStateMultiPart[] Parts { get; set; } = new BlockStateMultiPart[0];
	}
}
