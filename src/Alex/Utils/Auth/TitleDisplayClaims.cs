using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class TitleDisplayClaims
	{
		[JsonProperty("xti")] public XTI Xti { get; set; }
	}
}