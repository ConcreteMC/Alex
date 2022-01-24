using System;
using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser.Parselet
{
	public class NameParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			var args = parser.ParseArgs();
			string name = parser.FixNameShortcut(token.Text);

			IExpression nameExpr = new NameExpression(name);

			if (args.Count > 0)
			{
				return new FuncCallExpression(nameExpr, args.ToArray());
			}

			return nameExpr;
		}
	}
}