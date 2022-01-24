using System.Collections.Generic;
using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class AuthRequest
	{
		[JsonProperty("RelyingParty")] public string RelyingParty { get; set; }

		[JsonProperty("TokenType")] public string TokenType { get; set; }

		[JsonProperty("Properties")] public Dictionary<string, object> Properties { get; set; }
	}
}