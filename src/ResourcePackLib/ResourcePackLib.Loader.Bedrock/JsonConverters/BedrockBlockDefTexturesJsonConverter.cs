using Newtonsoft.Json;

namespace ResourcePackLib.Loader.Bedrock.Data;

public class BedrockBlockDefTexturesJsonConverter : JsonConverter<BedrockBlockDefTextures>
{
    public override void WriteJson(JsonWriter writer, BedrockBlockDefTextures? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override BedrockBlockDefTextures? ReadJson(JsonReader reader, Type objectType, BedrockBlockDefTextures? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            var strMap = serializer.Deserialize<Dictionary<string, string>>(reader);
            if (strMap == null)
                throw new InvalidOperationException();

            var t = new BedrockBlockDefTextures();
            if (strMap.ContainsKey("up"))
                t.Up = strMap["up"];

            if (strMap.ContainsKey("down"))
                t.Down = strMap["down"];

            if (strMap.ContainsKey("side"))
                t.North = t.East = t.South = t.West = strMap["side"];

            if (strMap.ContainsKey("north"))
                t.Down = strMap["north"];
            if (strMap.ContainsKey("east"))
                t.East = strMap["east"];
            if (strMap.ContainsKey("south"))
                t.South = strMap["south"];
            if (strMap.ContainsKey("west"))
                t.West = strMap["west"];

            return t;
        }
        else if (reader.TokenType == JsonToken.String)
        {
            var strValue = serializer.Deserialize<string>(reader);
            if (strValue == null)
                throw new InvalidOperationException();

            return new BedrockBlockDefTextures(strValue);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}