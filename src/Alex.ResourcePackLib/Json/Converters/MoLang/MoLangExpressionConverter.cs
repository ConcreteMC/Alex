using System;
using System.Collections.Generic;
using Alex.MoLang.Parser;
using Alex.MoLang.Parser.Expressions;
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

				var res = parser.Parse();

				return res;
			}
			else if (token.Type == JTokenType.Integer)
			{
				return new IExpression[]
				{
					new NumberExpression(token.Value<double>())
				};
			}
			else if (token.Type == JTokenType.Float)
			{
				return new IExpression[]
				{
					new NumberExpression(token.Value<double>())
				};
			}
			else if (token.Type == JTokenType.Boolean)
			{
				return new IExpression[]
				{
					new BooleanExpression(token.Value<bool>())
				};
			}

			return null;
		}

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			return typeof(IExpression[]).IsAssignableFrom(objectType);
		}


		/// <inheritdoc />
		public override bool CanWrite => false;
	}
}