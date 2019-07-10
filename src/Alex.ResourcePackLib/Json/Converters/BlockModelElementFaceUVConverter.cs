using System;
using System.Linq;
using Alex.ResourcePackLib.Json.Models.Blocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters
{
	public class BlockModelElementFaceUVConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var uv = value as BlockModelElementFaceUV;

			serializer.Serialize(writer, new int[]
			{
				uv.X1,
				uv.Y1,
				uv.X2,
				uv.Y2
			});
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.Array)
			{
				var arr = (JArray)obj;
				if (arr.Count == 4 && arr.All(token => token.Type == JTokenType.Integer))
				{
					return new BlockModelElementFaceUV()
					{
						X1 = arr[0].Value<int>(),
						Y1 = arr[1].Value<int>(),
						X2 = arr[2].Value<int>(),
						Y2 = arr[3].Value<int>()
					};
				}
			}

			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(BlockModelElementFaceUV).IsAssignableFrom(objectType);
		}
	}
}
