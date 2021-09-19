using System;
using Alex.MoLang.Parser;
using Alex.ResourcePackLib.Json.Bedrock.Entity;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters.Bedrock
{
	internal class ComplexStuffConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(AnimationChannelData);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);
			
			switch (obj.Type)
			{
				case JTokenType.Array:
					var expressions = obj.ToObject<IExpression[][]>(serializer);

					return new AnimationChannelData()
					{
						Expressions = expressions
					};

				case JTokenType.Object:
					var frameValue = obj.ToObject<AnimationKeyFrame>(serializer);

					return new AnimationChannelData()
					{
						KeyFrame = frameValue
					};
			}

			throw new Exception("Cannot unmarshal type AnimateElement");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			
			throw new Exception("Cannot marshal type ComplexStuff");
		}
	}
}