using System;
using System.Collections.Generic;
using Alex.MoLang.Parser;
using Alex.ResourcePackLib.Json.Bedrock.MoLang;
using Alex.ResourcePackLib.Json.Converters.MoLang;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters.Bedrock
{
	public class MoVec3Converter : JsonConverter<MoLangVector3Expression>
	{
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, MoLangVector3Expression value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override MoLangVector3Expression ReadJson(JsonReader reader,
			Type objectType,
			MoLangVector3Expression existingValue,
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

						return new MoLangVector3Expression(values);
					}
					break;
				case JTokenType.Object:
					if (obj is JObject jObject)
					{
						return new MoLangVector3Expression(
							jObject.ToObject<Dictionary<string, AnimationChannelData>>(
								new JsonSerializer()
								{
									Converters = { new MoLangExpressionConverter()}
								}));
					}
					break;
			}
			
			var raw = obj.ToObject<IExpression[]>(MCJsonConvert.Serializer);

			return new MoLangVector3Expression(new IExpression[][]
			{
				raw
			});
			
			throw new Exception("No.");
		}
	}
	
	public class MoVec4Converter : JsonConverter<MoLangVector4Expression>
	{
		/// <inheritdoc />
		public override bool CanWrite => false;

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, MoLangVector4Expression value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override MoLangVector4Expression ReadJson(JsonReader reader,
			Type objectType,
			MoLangVector4Expression existingValue,
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

						return new MoLangVector4Expression(values);
					}
					break;
				case JTokenType.Object:
					if (obj is JObject jObject)
					{
						return new MoLangVector4Expression(
							jObject.ToObject<Dictionary<string, AnimationChannelData>>(
								new JsonSerializer()
								{
									Converters = { new MoLangExpressionConverter()}
								}));
					}
					break;
			}
			
			var raw = obj.ToObject<IExpression[]>(MCJsonConvert.Serializer);

			return new MoLangVector4Expression(new IExpression[][]
			{
				raw
			});
			
			throw new Exception("No.");
		}
	}
}