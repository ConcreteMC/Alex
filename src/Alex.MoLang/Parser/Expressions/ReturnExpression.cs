using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class ReturnExpression : Expression
	{
		private readonly IExpression _value;

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			IMoValue eval = _value.Evaluate(scope, environment);
			scope.ReturnValue = eval;

			return eval;
		}

		/// <inheritdoc />
		public ReturnExpression(IExpression value) : base()
		{
			_value = value;
		}
	}
}