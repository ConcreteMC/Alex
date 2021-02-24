using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions.BinaryOp
{
	public class ArrowExpression : BinaryOpExpression
	{
		public ArrowExpression(IExpression left, IExpression right) : base(left, right) {
			
		}
		
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			var                 leftEnv = Left.Evaluate(scope, environment);
			if (leftEnv is MoLangEnvironment) {
				return Right.Evaluate(scope, (MoLangEnvironment) leftEnv);
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