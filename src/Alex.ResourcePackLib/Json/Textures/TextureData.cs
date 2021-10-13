using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Textures
{
	public class TextureData
	{
		[JsonProperty("blur")]
		public bool Blur { get; set; }

		[JsonProperty("clamp")]
		public bool Clamp { get; set; }
	}
}