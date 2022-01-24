using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions.BinaryOp
{
	public class ArrowExpression : BinaryOpExpression
	{
		public ArrowExpression(IExpression left, IExpression right) : base(left, right) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			var leftEnv = Left.Evaluate(scope, environment);

			if (leftEnv is MoLangEnvironment leftMolangEnvironment)
			{
				return Right.Evaluate(scope, leftMolangEnvironment);
			}

			return null;
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "->";
		}
	}
}