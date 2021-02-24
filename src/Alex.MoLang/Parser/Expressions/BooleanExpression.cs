using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class BooleanExpression : Expression<bool>
	{
		public BooleanExpression(bool value) : base(value)
		{
			
		}
		
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(Value ? 1d : 0d);
		}
	}

	public class BooleanNotExpression : Expression<IExpression>
	{
		public BooleanNotExpression(IExpression value) : base(value)
		{
			//Value = value;
		}
		
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return Value.Evaluate(scope, environment).Equals(DoubleValue.One) ? DoubleValue.Zero : DoubleValue.One;
		}
	}
}