using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Textures
{
	public class TextureAnimation
	{
		[JsonProperty("interpolate")]
		public bool Interpolate { get; set; }

		[JsonProperty("width")]
		public long Width { get; set; }

		[JsonProperty("height")]
		public long Height { get; set; }

		[JsonProperty("frametime")] public int Frametime { get; set; } = 1;

		[JsonProperty("frames")]
		public FrameElement[] Frames { get; set; }
	}
}