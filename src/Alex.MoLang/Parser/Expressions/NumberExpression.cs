using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class NumberExpression : Expression<DoubleValue>
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return Value;
		}

		/// <inheritdoc />
		public NumberExpression(double value) : base(new DoubleValue(value)) { }
	}
}