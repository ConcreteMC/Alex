using System.Linq;
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

			if (array is VariableStruct vs)
			{
				MoScope subScope = new MoScope();

				foreach (IMoValue value in vs.Map.Values)
				{
					subScope.IsContinue = false;
					subScope.IsBreak = false;

					Variable.Assign(
						subScope, environment, value is VariableStruct vss ? vss.Map.FirstOrDefault().Value : value);

					Body.Evaluate(subScope, environment);

					if (subScope.ReturnValue != null)
					{
						return subScope.ReturnValue;
					}
					else if (subScope.IsBreak)
					{
						break;
					}
				}
			}

			return DoubleValue.Zero;
		}
	}
}