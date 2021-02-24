using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class NumberExpression : Expression<double>
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(Value);
		}

		/// <inheritdoc />
		public NumberExpression(double value) : base(value) { }
	}
}