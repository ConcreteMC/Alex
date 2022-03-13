using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Fonts
{
	public class FontDefinitionFile
	{
		[JsonProperty("providers")]
		public FontDefinition[] Providers { get; set; }
	}
}