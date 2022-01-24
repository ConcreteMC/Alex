using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions.BinaryOp
{
	public class EqualExpression : BinaryOpExpression
	{
		/// <inheritdoc />
		public EqualExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			var left = Left.Evaluate(scope, environment);
			var right = Right.Evaluate(scope, environment);

			return new DoubleValue(left.Equals(right));
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "==";
		}
	}
}