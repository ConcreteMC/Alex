using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions.BinaryOp
{
	public class SmallerExpression : BinaryOpExpression
	{
		/// <inheritdoc />
		public SmallerExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(
				Left.Evaluate(scope, environment).AsDouble() < Right.Evaluate(scope, environment).AsDouble());
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "<";
		}
	}
}