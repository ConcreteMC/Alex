using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class BooleanExpression : Expression<DoubleValue>
	{
		public BooleanExpression(bool value) : base(value ? DoubleValue.One : DoubleValue.Zero) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return Value;
		}
	}
}