using Newtonsoft.Json;

namespace Alex.Worlds.Multiplayer.Bedrock
{

    public class CertificateData
    {
        public const string MojangRootKey = "MHYwEAYHKoZIzj0CAQYFK4EEACIDYgAE8ELkixyLcwlZryUQcu1TvPOmI2B7vX83ndnWRUaXm74wFfa5f/lwQNTfrLVHa2PmenpGI6JhIMUJaWZrjmMj90NoKNFSNBuKdm8rYiXsfaz3K36x/1U26HpG0ZxK/V1V";

        public long Nbf { get; set; }

        [JsonProperty("extraData")]
        public ExtraData ExtraData { get; set; }

        public long RandomNonce { get; set; }

        public string Iss { get; set; }

        public long Exp { get; set; }

        public long Iat { get; set; }

        public bool CertificateAuthority { get; set; }

        [JsonProperty("identityPublicKey")]
        public string IdentityPublicKey { get; set; }
    }

    public class ExtraData
    {
        [JsonProperty("identity")]
        public string Identity { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("XUID")]
        public string XUID { get; set; }
    }
}
