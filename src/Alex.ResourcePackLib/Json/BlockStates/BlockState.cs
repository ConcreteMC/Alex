using System.Collections.Generic;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.BlockStates
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
