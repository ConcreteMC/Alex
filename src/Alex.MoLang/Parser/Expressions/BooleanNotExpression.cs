using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class BooleanNotExpression : Expression
	{
		private readonly IExpression _value;

		public BooleanNotExpression(IExpression value) : base()
		{
			_value = value;
			//Value = value;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return _value.Evaluate(scope, environment).AsBool() ? DoubleValue.Zero :
				DoubleValue.One; // .Equals(DoubleValue.One) ? DoubleValue.Zero : DoubleValue.One;
		}
	}
}