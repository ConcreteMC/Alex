using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class ForEachExpression : Expression<IExpression>
	{
		IExpression _variable;
		IExpression _array;
		IExpression _body;

		public ForEachExpression(IExpression variable, IExpression array, IExpression body) : base(null)
		{
			_variable = variable;
			_array = array;
			_body = body;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			IMoValue array = _array.Evaluate(scope, environment);

			if (array is VariableStruct vs) {
				MoScope scope2 = new MoScope();

				foreach (IMoValue value in vs.Map.Values) {
					_variable.Assign(scope2, environment, value);
					_body.Evaluate(scope2, environment);

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