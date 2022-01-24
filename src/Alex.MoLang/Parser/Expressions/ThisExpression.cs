using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;

namespace Alex.MoLang.Parser.Expressions
{
	public class ThisExpression : Expression
	{
		public static readonly MoPath _this = new MoPath("this");

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return environment.ThisVariable; // environment.GetValue(_this);
		}

		/// <inheritdoc />
		public ThisExpression() { }
	}
}