using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class MsaDeviceAuthPollState : BedrockTokenPair
	{
		[JsonProperty("user_id")] public string UserId;

		[JsonProperty("token_type")] public string TokenType;

		[JsonProperty("scope")] public string Scope;

		//public int interval;
		[JsonProperty("expires_in")] public int ExpiresIn;

		[JsonProperty("error")] public string Error;
	};
}