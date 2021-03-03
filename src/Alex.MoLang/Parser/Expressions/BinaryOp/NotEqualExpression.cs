using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions.BinaryOp
{
	public class NotEqualExpression : BinaryOpExpression
	{
		/// <inheritdoc />
		public NotEqualExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			 return new DoubleValue(!Left.Evaluate(scope, environment).Value.Equals(Right.Evaluate(scope, environment).Value));
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "!=";
		}
	}
}