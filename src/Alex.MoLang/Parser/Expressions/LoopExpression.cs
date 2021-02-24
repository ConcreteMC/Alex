using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class LoopExpression : Expression<IExpression>
	{
		public IExpression Count;
		public IExpression Body;

		public LoopExpression(IExpression count, IExpression body) : base(null)
		{
			Count = count;
			Body = body;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			int     loop     = (int) (double)Count.Evaluate(scope, environment).Value;
			MoScope subScope = new MoScope();

			while (loop > 0) {
				Body.Evaluate(subScope, environment);
				loop--;

				if (subScope.ReturnValue != null) {
					return subScope.ReturnValue;
				} else if (subScope.IsBreak) {
					break;
				}
			}

			return DoubleValue.Zero;
		}
	}
}