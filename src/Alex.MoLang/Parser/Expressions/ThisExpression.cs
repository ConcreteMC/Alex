using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class ThisExpression : Expression<IExpression>
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return environment;
		}

		/// <inheritdoc />
		public ThisExpression() : base(null) { }
	}
}