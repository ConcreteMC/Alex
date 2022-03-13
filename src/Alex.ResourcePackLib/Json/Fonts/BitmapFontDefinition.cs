using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Fonts
{
	public class BitmapFontDefinition : FontDefinition
	{
		[JsonProperty("file")]
		public string File { get; set; }
		
		[JsonProperty("height")]
		public int? Height { get; set; }
		
		[JsonProperty("ascent")]
		public int? Ascent { get; set; }

		[JsonProperty("chars")] public string[] Characters { get; set; }
	}
}