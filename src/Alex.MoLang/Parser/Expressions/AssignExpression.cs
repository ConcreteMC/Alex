using System.Collections.Generic;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class AssignExpression : Expression<IExpression>
	{
		private IExpression _variable;
		private IExpression _expr;

		public AssignExpression(IExpression variable, IExpression expr) : base(null)
		{
			_variable = variable;
			_expr = expr;
		}
		
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			_variable.Assign(scope, environment, _expr.Evaluate(scope, environment));

			return DoubleValue.Zero;
		}
	}
}