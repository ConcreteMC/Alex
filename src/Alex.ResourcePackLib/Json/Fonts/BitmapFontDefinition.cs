using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Fonts
{
	public class BitmapFontDefinition : FontDefinition
	{
		public string File { get; set; }
		public int? Height { get; set; }
		public int? Ascent { get; set; }

		[JsonProperty("chars")] public string[] Characters { get; set; }
	}
}