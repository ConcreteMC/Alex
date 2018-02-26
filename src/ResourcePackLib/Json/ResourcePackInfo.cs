using System.Drawing;
using Newtonsoft.Json;

namespace ResourcePackLib.Json
{
	internal class ResourcePackInfoWrapper
	{
		public ResourcePackInfo pack;
	}

	public class ResourcePackInfo
	{
		[JsonIgnore]
		public Bitmap Logo;

		[JsonProperty("pack_format")]
		public int Format;

		[JsonProperty("description")]
		public string Description;
	}
}
