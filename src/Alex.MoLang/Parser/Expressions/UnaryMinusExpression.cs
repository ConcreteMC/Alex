using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class UnaryMinusExpression : Expression
	{
		private readonly IExpression _value;

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(-(_value.Evaluate(scope, environment).AsDouble()));
		}

		/// <inheritdoc />
		public UnaryMinusExpression(IExpression value)
		{
			_value = value;
		}
	}
}