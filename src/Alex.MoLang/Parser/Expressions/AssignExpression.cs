using System.Collections.Generic;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class AssignExpression : Expression
	{
		public IExpression Variable { get; set; }
		public IExpression Expression { get; set; }

		public AssignExpression(IExpression variable, IExpression expr)
		{
			Variable = variable;
			Expression = expr;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			Variable.Assign(scope, environment, Expression.Evaluate(scope, environment));

			return DoubleValue.Zero;
		}
	}
}