using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class DeviceDisplayClaims
	{
		[JsonProperty("xdi")]
		public XDI Xdi { get; set; }
	}
}