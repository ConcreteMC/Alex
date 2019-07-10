using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
    public class BlockFaceConverter : JsonConverter
    {
	    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	    {
			
	    }

	    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	    {
			var obj = JToken.Load(reader);
		    if (obj.Type == JTokenType.String)
		    {
			    string value = obj.Value<string>();
			    if (string.IsNullOrWhiteSpace(value)) return BlockFace.None;

				if (Enum.TryParse(value, out BlockFace result))
			    {
				    return result;
				}
			    else
			    {
				    return BlockFace.None;
			    }
		    }
			else if (obj.Type == JTokenType.Integer)
		    {
			    return (BlockFace) obj.Value<int>();
		    }

		    return BlockFace.None;
	    }

	    public override bool CanConvert(Type objectType)
	    {
		    return typeof(BlockFace).IsAssignableFrom(objectType);
	    }
    }
}
