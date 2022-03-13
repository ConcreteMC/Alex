using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Fonts
{
	public class LegacyFontDefinition : FontDefinition
	{
		[JsonProperty("sizes")]
		public string Sizes { get; set; }
		
		[JsonProperty("template")]
		public string Template { get; set; }
	}
}