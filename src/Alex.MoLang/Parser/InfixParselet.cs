using Alex.MoLang.Parser.Tokenizer;

namespace Alex.MoLang.Parser
{
	public abstract class InfixParselet
	{
		public Precedence Precedence { get; protected set; }

		public InfixParselet(Precedence precedence)
		{
			Precedence = precedence;
		}

		public abstract IExpression Parse(MoLangParser parser, Token token, IExpression leftExpr);
	}

	public abstract class PrefixParselet
	{
		public abstract IExpression Parse(MoLangParser parser, Token token);
	}
}