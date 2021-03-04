using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class ForEachExpression : Expression
	{
		public IExpression Variable { get; set; }
		public IExpression Array { get; set; }
		public IExpression Body { get; set; }

		public ForEachExpression(IExpression variable, IExpression array, IExpression body)
		{
			Variable = variable;
			Array = array;
			Body = body;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			IMoValue array = Array.Evaluate(scope, environment);

			if (array is VariableStruct vs) {
				MoScope scope2 = new MoScope();

				foreach (IMoValue value in vs.Map.Values) {
					Variable.Assign(scope2, environment, value);
					Body.Evaluate(scope2, environment);

					if (scope2.ReturnValue != null) {
						return scope2.ReturnValue;
					} else if (scope2.IsBreak) {
						break;
					}
				}
			}

			return DoubleValue.Zero;
		}
	}
}