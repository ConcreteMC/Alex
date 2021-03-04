using System;
using System.Collections.Generic;
using Alex.MoLang.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Bedrock.Entity
{
	internal class ComplexStuffConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(ComplexStuff);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);
			
			switch (obj.Type)
			{
				case JTokenType.Array:
					var expressions = obj.ToObject<IExpression[][]>(serializer);// serializer.Deserialize<List<IExpression>[]>(reader);

					return new ComplexStuff()
					{
						Expressions = expressions
					};

				case JTokenType.Object:
					var frameValue = obj.ToObject<PrePostKeyFrame>(serializer);// serializer.Deserialize<PrePostKeyFrame>(reader);

					return new ComplexStuff()
					{
						Frame = frameValue
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