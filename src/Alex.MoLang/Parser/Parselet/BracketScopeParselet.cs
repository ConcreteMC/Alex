using System.Collections.Generic;
using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser.Parselet
{
	public class BracketScopeParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			List<IExpression> exprs = new List<IExpression>();

			if (!parser.MatchToken(TokenType.CurlyBracketRight))
			{
				do
				{
					if (parser.MatchToken(TokenType.CurlyBracketRight, false))
					{
						break;
					}

					exprs.Add(parser.ParseExpression(Precedence.Scope));
				} while (parser.MatchToken(TokenType.Semicolon));

				parser.ConsumeToken(TokenType.CurlyBracketRight);
			}

			return new StatementExpression(exprs.ToArray());
		}
	}
}