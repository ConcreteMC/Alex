using System;
using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using ConcreteMC.MolangSharp.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Converters.MoLang
{
	internal class AnnoyingMolangElementConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(AnnoyingMolangElement);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.String:
				case JsonToken.Date:
					var stringValue = serializer.Deserialize<string>(reader);

					return new AnnoyingMolangElement(stringValue);

				case JsonToken.StartObject:
					var objectValue = serializer.Deserialize<Dictionary<string, IExpression>>(reader);

					return new AnnoyingMolangElement(objectValue);
			}

			throw new Exception("Cannot unmarshal type AnimateElement");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			var value = (AnnoyingMolangElement)untypedValue;

			if (value.StringValue != null)
			{
				serializer.Serialize(writer, value.StringValue);

				return;
			}

			if (value.Expressions != null)
			{
				serializer.Serialize(writer, value.Expressions);

				return;
			}

			throw new Exception("Cannot marshal type AnimateElement");
		}
	}
}