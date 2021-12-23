using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class StatementExpression : Expression<IExpression[]>
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			foreach (IExpression expression in Value) {
				expression.Evaluate(scope, environment);

				if (scope.ReturnValue != null) {
					return scope.ReturnValue;
				} else if (scope.IsBreak || scope.IsContinue) {
					break;
				}
			}
			
			return DoubleValue.Zero;
		}

		/// <inheritdoc />
		public StatementExpression(IExpression[] value) : base(value) { }
	}
}