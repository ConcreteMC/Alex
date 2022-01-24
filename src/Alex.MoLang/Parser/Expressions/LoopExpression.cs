using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class LoopExpression : Expression
	{
		public IExpression Count { get; set; }
		public IExpression Body { get; set; }

		public LoopExpression(IExpression count, IExpression body)
		{
			Count = count;
			Body = body;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			int loop = (int)(double)Count.Evaluate(scope, environment).Value;
			MoScope subScope = new MoScope();

			while (loop > 0)
			{
				subScope.IsContinue = false;
				subScope.IsBreak = false;

				Body.Evaluate(subScope, environment);
				loop--;

				if (subScope.ReturnValue != null)
				{
					return subScope.ReturnValue;
				}
				else if (subScope.IsBreak)
				{
					break;
				}
			}

			return DoubleValue.Zero;
		}
	}
}