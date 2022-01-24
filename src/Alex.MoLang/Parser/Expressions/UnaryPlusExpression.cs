using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class UnaryPlusExpression : Expression
	{
		private readonly IExpression _value;

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(+(_value.Evaluate(scope, environment).AsDouble()));
		}

		/// <inheritdoc />
		public UnaryPlusExpression(IExpression value) : base()
		{
			_value = value;
		}
	}
}