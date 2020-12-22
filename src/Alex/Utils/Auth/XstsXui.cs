using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class XstsXui
	{
		//	[JsonProperty("agg")]
		//	public string AgeGroup { get; set; }
			
		[JsonProperty("gtg")]
		public string Gamertag { get; set; }

		[JsonProperty("xid")]
		public string XUID { get; set; }
			
		[JsonProperty("uhs")]
		public string UserHash { get; set; }
	}
}