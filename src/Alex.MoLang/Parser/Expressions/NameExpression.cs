using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class NameExpression : Expression<string>
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return environment.GetValue(Value);
		}

		/// <inheritdoc />
		public override void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value)
		{
			environment.SetValue(Value, value);
		}

		/// <inheritdoc />
		public NameExpression(string value) : base(value)
		{
			
		}
	}
}