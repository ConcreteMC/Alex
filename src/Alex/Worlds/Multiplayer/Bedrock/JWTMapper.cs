using Jose;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Alex.Worlds.Multiplayer.Bedrock
{
    public class JWTMapper : IJsonMapper
    {
        private static DefaultContractResolver ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };

        public string Serialize(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
            //    ContractResolver = ContractResolver

            };

            return JsonConvert.SerializeObject(obj, Formatting.Indented, settings);
        }

        public T Parse<T>(string json)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
              //  ContractResolver = ContractResolver
            };

            return JsonConvert.DeserializeObject<T>(json, settings);
        }
    }
}
