using System;
using System.Collections.Generic;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Tokenizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.ResourcePackLib.Json.Converters.MoLang
{
	public class MoLangExpressionConverter : JsonConverter
	{
		public MoLangExpressionConverter()
		{
			
		}
		
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			JToken token = JToken.Load(reader);

			if (token.Type == JTokenType.String)
			{
				string        molang        = token.Value<string>();
				TokenIterator tokenIterator = new TokenIterator(molang);
				MoLangParser  parser        = new MoLangParser(tokenIterator);

				return parser.Parse();
			}

			return null;
		}

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(List<IExpression>).IsAssignableFrom(objectType);
		}


		/// <inheritdoc />
		public override bool CanWrite => false;
	}
}