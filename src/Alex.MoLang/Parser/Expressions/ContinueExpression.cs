using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class ContinueExpression : Expression
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			scope.IsContinue = true;

			return DoubleValue.Zero;
		}
	}
}