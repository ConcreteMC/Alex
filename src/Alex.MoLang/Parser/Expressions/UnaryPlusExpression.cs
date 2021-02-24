using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class UnaryPlusExpression : Expression<IExpression>
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(+(Value.Evaluate(scope, environment).AsDouble()));
		}

		/// <inheritdoc />
		public UnaryPlusExpression(IExpression value) : base(value) { }
	}
}