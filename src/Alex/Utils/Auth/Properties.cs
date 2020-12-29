using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class Properties
	{
		[JsonProperty("AuthMethod")]
		public string AuthMethod { get; set; }

		[JsonProperty("SiteName")]
		public string SiteName { get; set; }

		[JsonProperty("RpsTicket")]
		public string RpsTicket { get; set; }
	}
}