using System.Collections.Generic;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Parser.Expressions
{
	public class StringExpression : Expression<string>
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new StringValue(Value);
		}

		/// <inheritdoc />
		public StringExpression(string value) : base(value) { }
	}
}