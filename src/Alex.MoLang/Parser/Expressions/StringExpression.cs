using System.Collections.Generic;
using Alex.MoLang.Runtime;
using Alex.MoLang.Runtime.Value;
using NLog;

namespace Alex.MoLang.Parser.Expressions
{
	public class StringExpression : Expression
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(StringExpression));

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new StringValue(_value);
		}

		private string _value;

		/// <inheritdoc />
		public StringExpression(string value) : base()
		{
			_value = value;
		}
	}
}