using Alex.MoLang.Parser.Expressions;
using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser.Parselet
{
	public class AssignParselet : InfixParselet
	{
		/// <inheritdoc />
		public AssignParselet() : base(Precedence.Assignment) { }

		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token, IExpression leftExpr)
		{
			return new AssignExpression(leftExpr, parser.ParseExpression(Precedence));
		}
	}
}