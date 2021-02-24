using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions.BinaryOp
{
	public class BooleanOrExpression : BinaryOpExpression
	{
		/// <inheritdoc />
		public BooleanOrExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(!Left.Evaluate(scope, environment).Equals(DoubleValue.Zero) || !Right.Evaluate(scope, environment).Equals(DoubleValue.Zero));
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "||";
		}
	}
}