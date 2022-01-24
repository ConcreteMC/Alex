using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Textures
{
	public class TextureMeta
	{
		[JsonProperty("animation")] public TextureAnimation Animation { get; set; }

		[JsonProperty("texture")] public TextureData Texture { get; set; }

		public static TextureMeta FromJson(string json) =>
			JsonConvert.DeserializeObject<TextureMeta>(json, Converter.Settings);
	}
}