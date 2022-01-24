using System;
using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class BedrockTokenPair
	{
		[JsonProperty("access_token")] public string AccessToken;

		[JsonProperty("refresh_token")] public string RefreshToken;

		[JsonProperty("expiry_time")] public DateTime ExpiryTime;

		[JsonIgnore] public string DeviceId;
	}
}