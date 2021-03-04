using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class NameExpression : Expression
	{
		public string Name { get; set; }
		
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return environment.GetValue(Name);
		}

		/// <inheritdoc />
		public override void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value)
		{
			environment.SetValue(Name, value);
		}

		/// <inheritdoc />
		public NameExpression(string value)
		{
			Name = value;
		}
	}
}