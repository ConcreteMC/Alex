using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class BreakExpression : Expression<IExpression>
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			scope.IsBreak = true;
			return DoubleValue.Zero;
		}

		/// <inheritdoc />
		public BreakExpression() : base(null) { }
	}
}