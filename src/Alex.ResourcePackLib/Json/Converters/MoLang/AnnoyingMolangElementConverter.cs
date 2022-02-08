using System;
using System.Collections.Generic;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using ConcreteMC.MolangSharp.Parser;
using Newtonsoft.Json;

namespace Alex.ResourcePackLib.Json.Converters.MoLang
{
	internal class AnnoyingMolangElementConverter : JsonConverter<AnnoyingMolangElement>
	{
		public override AnnoyingMolangElement ReadJson(JsonReader reader, Type t, AnnoyingMolangElement existingValue, bool hasExistingValue, JsonSerializer serializer)
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

		public override void WriteJson(JsonWriter writer, AnnoyingMolangElement value, JsonSerializer serializer)
		{
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