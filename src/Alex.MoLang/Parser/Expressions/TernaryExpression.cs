using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class TernaryExpression : Expression<IExpression>
	{
		public IExpression Condition;
		public IExpression ThenExpr;
		public IExpression ElseExpr;

		public TernaryExpression(IExpression condition, IExpression thenExpr, IExpression elseExpr) : base(null)
		{
			Condition = condition;
			ThenExpr = thenExpr;
			ElseExpr = elseExpr;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			if (Condition.Evaluate(scope, environment).Equals(DoubleValue.One)) {
				return ThenExpr == null ? Condition.Evaluate(scope, environment) : ThenExpr.Evaluate(scope, environment);
			} else if (ElseExpr != null) {
				return ElseExpr.Evaluate(scope, environment);
			}

			return DoubleValue.Zero;
		}
	}
}