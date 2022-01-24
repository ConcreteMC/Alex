using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Fonts
{
	public class FontDefinition
	{
		[JsonProperty("type")] public string Type { get; set; }

		/*[JsonProperty("file")]
		public string File { get; set; }
		
		[JsonProperty("ascent")]
		public int Ascent { get; set; }
		
		[JsonProperty("height")]
		public int Height { get; set; }
		
		[JsonProperty("chars")]
		public string[] Chars { get; set; }
		
		[JsonProperty("sizes")]
		public string Sizes { get; set; }
		
		[JsonProperty("template")]
		public string Template { get; set; }*/
	}
}