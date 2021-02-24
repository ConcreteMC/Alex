using Alex.MoLang.Parser.Expressions.BinaryOp;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser.Parselet
{
	public class GenericBinaryOpParselet : InfixParselet
	{
		/// <inheritdoc />
		public GenericBinaryOpParselet(Precedence precedence) : base(precedence) { }

		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token, IExpression leftExpr)
		{
			IExpression rightExpr = parser.ParseExpression(Precedence);

			if (token.Type == TokenType.Arrow)
			{
				return new ArrowExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.And)
			{
				return new BooleanAndExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.Or)
			{
				return new BooleanOrExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.Coalesce)
			{
				return new CoalesceExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.Slash)
			{
				return new DivideExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.EqualsEquals)
			{
				return new EqualExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.Greater)
			{
				return new GreaterExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.GreaterOrEquals)
			{
				return new GreaterOrEqualExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.Minus)
			{
				return new MinusExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.NotEquals)
			{
				return new NotEqualExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.Plus)
			{
				return new PlusExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.Asterisk)
			{
				return new PowExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.Smaller)
			{
				return new SmallerExpression(leftExpr, rightExpr);
			}
			else if (token.Type == TokenType.SmallerOrEquals)
			{
				return new SmallerOrEqualExpression(leftExpr, rightExpr);
			}

			return null;
		}
	}
}