using Newtonsoft.Json;

namespace Alex.Utils.Auth
{
	public class ProofKey
	{
		[JsonProperty("crv")] public string Crv { get; set; } = "P-256";

		[JsonProperty("alg")] public string Algorithm { get; set; } = "ES256";

		[JsonProperty("use")] public string Use { get; set; } = "sig";

		[JsonProperty("kty")] public string Kty { get; set; } = "EC";

		[JsonProperty("x")] public string X { get; set; }

		[JsonProperty("y")] public string Y { get; set; }

		public ProofKey(string x, string y)
		{
			X = x;
			Y = y;
		}
	}

	/*
	 * "ProofKey", new Dictionary<string, string>()
						{
							{"crv", "P-256"},
							{"alg", "ES256"},
							{"use", "sig"},
							{"kty", "EC"},
							{"x",UrlSafe(X)},
							{"y", UrlSafe(Y)}
						}
	 */
}