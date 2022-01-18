using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResourcePackLib.Core.Json.Converters;

public class Vector3ArrayJsonConverter : JsonConverter<Vector3?>
{
    public override void WriteJson(JsonWriter writer, Vector3? value, JsonSerializer serializer)
    {
        if (value.HasValue)
        {
            writer.WriteRawValue(JsonConvert.SerializeObject(new float[]
            {
                value.Value.X, value.Value.Y, value.Value.Z
            }, Formatting.None));
        }
        else
        {
            writer.WriteNull();
        }
        /*serializer.Serialize(writer, new float[]
        {
            v.X,
            v.Y,
            v.Z
        });*/
    }
    public override Vector3? ReadJson(JsonReader reader, Type objectType, Vector3? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var obj = JToken.Load(reader);

        if (obj.Type == JTokenType.Array)
        {
            var arr = (JArray)obj;
            if (arr.Count == 3)
            {
                var v3 = new Vector3();

                if (arr[0].Type == JTokenType.Integer)
                {
                    v3.X = arr[0].Value<int>();
                }
                else if (arr[0].Type == JTokenType.Float)
                {
                    v3.X = arr[0].Value<float>();
                }
					
                if (arr[1].Type == JTokenType.Integer)
                {
                    v3.Y = arr[1].Value<int>();
                }
                else if (arr[1].Type == JTokenType.Float)
                {
                    v3.Y = arr[1].Value<float>();
                }

                if (arr[2].Type == JTokenType.Integer)
                {
                    v3.Z = arr[2].Value<int>();
                }
                else if (arr[2].Type == JTokenType.Float)
                {
                    v3.Z = arr[2].Value<float>();
                }

                return v3;
            }
        }

        return null;
    }
}