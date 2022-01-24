using System;
using Alex.MoLang.Parser;

namespace Alex.MoLang.Runtime.Exceptions
{
	public class MissingQueryMethodException : MoLangRuntimeException
	{
		/// <inheritdoc />
		public MissingQueryMethodException(string message, Exception baseException) : base(message, baseException) { }

		/// <inheritdoc />
		public MissingQueryMethodException(IExpression expression, string message, Exception baseException) : base(
			expression, message, baseException) { }
	}
}