using System;
using System.Globalization;
using Alex.MoLang.Parser.Exceptions;
using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Parser.Tokenizer;
using csFastFloat;

namespace Alex.MoLang.Parser.Parselet
{
	public class NumberParselet : PrefixParselet
	{
		private const NumberStyles NumberStyle = System.Globalization.NumberStyles.AllowDecimalPoint;
		private static readonly CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			if (FastDoubleParser.TryParseDouble(token.Text, out double result))
			{
				return new NumberExpression(result);
			}

			throw new MoLangParserException($"Could not parse \'{token.Text.ToString()}\' as a double");

			return new NumberExpression(Double.Parse(token.Text));
		}
	}
}