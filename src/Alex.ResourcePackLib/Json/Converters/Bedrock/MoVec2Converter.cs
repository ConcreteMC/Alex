using System;
using System.Collections.Generic;
using Alex.MoLang.Parser;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Alex.ResourcePackLib.Json.Converters.MoLang;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters.Bedrock
{
	public class MoVec2Converter : JsonConverter<MoLangVector2Expression>
	{
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, MoLangVector2Expression value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override MoLangVector2Expression ReadJson(JsonReader reader,
			Type objectType,
			MoLangVector2Expression existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			switch (obj.Type)
			{
				case JTokenType.Array:
					if (obj is JArray jArray)
					{
						IExpression[][] values = jArray.ToObject<IExpression[][]>(MCJsonConvert.Serializer);

						return new MoLangVector2Expression(values);
					}
					break;
				case JTokenType.Object:
					if (obj is JObject jObject)
					{
						return new MoLangVector2Expression(
							jObject.ToObject<Dictionary<string, ComplexStuff>>(
								new JsonSerializer()
								{
									Converters = { new MoLangExpressionConverter()}
								}));
					}
					break;
			}
			
			var raw = obj.ToObject<IExpression[]>(MCJsonConvert.Serializer);

			return new MoLangVector2Expression(new IExpression[][]
			{
				raw
			});
			
			throw new Exception("No.");
		}
	}
}