using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser.Parselet
{
	public class ArrayAccessParselet : InfixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token, IExpression leftExpr)
		{
			IExpression index = parser.ParseExpression(Precedence);
			parser.ConsumeToken(TokenType.ArrayRight);

			return new ArrayAccessExpression(leftExpr, index);
		}

		/// <inheritdoc />
		public ArrayAccessParselet() : base(Precedence.ArrayAccess) { }
	}
}