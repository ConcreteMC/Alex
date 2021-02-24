using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class UnaryMinusExpression : Expression<IExpression>
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(-(Value.Evaluate(scope, environment).AsDouble()));
		}

		/// <inheritdoc />
		public UnaryMinusExpression(IExpression value) : base(value) { }
	}
}