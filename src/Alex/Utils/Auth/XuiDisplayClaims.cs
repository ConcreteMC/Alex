using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class XuiDisplayClaims<TType>
	{
		[JsonProperty("xui")] public TType[] Xui { get; set; }
	}
}