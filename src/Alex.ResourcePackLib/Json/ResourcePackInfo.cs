using System.Drawing;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.ResourcePackLib.Json
{
	internal class ResourcePackInfoWrapper
	{
		public ResourcePackInfo pack;
	}

	public class ResourcePackInfo
	{
		[JsonIgnore]
		public Image<Rgba32> Logo;

		[JsonProperty("pack_format")]
		public int Format;

		[JsonProperty("description")]
		public string Description;
	}
}
