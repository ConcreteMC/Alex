using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class BreakExpression : Expression
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			scope.IsBreak = true;

			return DoubleValue.Zero;
		}
	}
}