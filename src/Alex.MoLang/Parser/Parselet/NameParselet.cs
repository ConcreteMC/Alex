using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser.Parselet
{
	public class NameParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			var    args = parser.ParseArgs();
			string name = parser.FixNameShortcut(token.Text);

			IExpression nameExpr = new NameExpression(name);

			if (args.Count > 0 || parser.GetNameHead(name).Equals("query") || parser.GetNameHead(name).Equals("math")){
				return new FuncCallExpression(nameExpr, args.ToArray());
			}

			return nameExpr;
		}
	}
}