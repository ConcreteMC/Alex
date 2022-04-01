using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Alex.ResourcePackLib.Json
{
	internal class ResourcePackInfoWrapper
	{
		[JsonProperty("pack")]
		public ResourcePackInfo Pack { get; set; }
	}

	public class ResourcePackInfo
	{
		[JsonIgnore] public Image<Rgba32> Logo;

		[JsonProperty("pack_format")] public int Format;

		[JsonProperty("description")] public string Description;
	}
}