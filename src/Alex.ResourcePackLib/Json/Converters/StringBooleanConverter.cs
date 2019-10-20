using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
    public class StringBooleanConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var v = value is string ? (string) value : ((bool)value).ToString().ToLower();

            serializer.Serialize(writer, v);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JToken.Load(reader);

            if (obj.Type == JTokenType.Boolean)
            {
                return obj.Value<bool>().ToString().ToLower();
            }
            else if (obj.Type == JTokenType.String)
            {
                return obj.Value<string>();
            }
            
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(string).IsAssignableFrom(objectType) 
                   || typeof(bool).IsAssignableFrom(objectType);
        }
    }
}