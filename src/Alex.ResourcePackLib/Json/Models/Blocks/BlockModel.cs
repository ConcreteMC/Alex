using System.Collections.Generic;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Models.Blocks
{
	public class BlockModel : ResourcePackModelBase
	{
		/// <summary>
		/// Whether to use ambient occlusion (true - default), or not (false).
		/// </summary>
		public bool AmbientOcclusion { get; set; } = true;
	}
}
