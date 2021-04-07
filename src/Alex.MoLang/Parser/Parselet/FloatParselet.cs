using System;
using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Parser.Tokenizer;
using csFastFloat;

namespace Alex.MoLang.Parser.Parselet
{
	public class FloatParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			return new NumberExpression(FastFloatParser.ParseFloat(token.Text));
		}
	}
}