using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class XDI
	{
		[JsonProperty("did")]
		public string DID { get; set; }
	}
}