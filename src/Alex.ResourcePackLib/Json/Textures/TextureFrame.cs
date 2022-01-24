using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Textures
{
	public class TextureFrame
	{
		[JsonProperty("index")] public long Index { get; set; }

		[JsonProperty("time")] public double Time { get; set; }
	}
}