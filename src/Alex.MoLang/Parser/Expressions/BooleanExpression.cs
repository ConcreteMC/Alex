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
			return Value ? DoubleValue.One : DoubleValue.Zero;
		}
	}
}