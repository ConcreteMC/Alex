using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions.BinaryOp
{
	public class EqualExpression :  BinaryOpExpression
	{
		/// <inheritdoc />
		public EqualExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(Left.Evaluate(scope, environment).Equals(Right.Evaluate(scope, environment)));
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "==";
		}
	}
}