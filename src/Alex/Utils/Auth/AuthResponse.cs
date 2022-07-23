using System;
using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class AuthResponse<TClaims>
	{
		[JsonIgnore] public bool Error { get; set; }
		[JsonIgnore] public int StatusCode { get; set; }
		[JsonProperty("IssueInstant")] public DateTimeOffset IssueInstant { get; set; }

		[JsonProperty("NotAfter")] public DateTimeOffset NotAfter { get; set; }

		[JsonProperty("Token")] public string Token { get; set; }

		[JsonProperty("DisplayClaims")] public TClaims DisplayClaims { get; set; }
	}
}