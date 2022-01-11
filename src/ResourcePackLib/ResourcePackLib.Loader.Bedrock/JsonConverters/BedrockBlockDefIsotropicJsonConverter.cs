using Newtonsoft.Json;

namespace ResourcePackLib.Loader.Bedrock.Data;

public class BedrockBlockDefIsotropicJsonConverter : JsonConverter<BedrockBlockDefIsotropic>
{
    public override void WriteJson(JsonWriter writer, BedrockBlockDefIsotropic? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override BedrockBlockDefIsotropic? ReadJson(JsonReader reader, Type objectType, BedrockBlockDefIsotropic? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            var strMap = serializer.Deserialize<Dictionary<string, bool>>(reader);
            if (strMap == null)
                throw new InvalidOperationException();

            return new BedrockBlockDefIsotropic(strMap["up"], strMap["down"]);
        }

        if (reader.TokenType == JsonToken.Boolean)
        {
            return new BedrockBlockDefIsotropic(serializer.Deserialize<bool>(reader));
        }

        throw new InvalidOperationException();
    }
}