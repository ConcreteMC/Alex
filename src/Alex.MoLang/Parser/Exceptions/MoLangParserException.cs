using System;

namespace Alex.MoLang.Parser.Exceptions
{
	public class MoLangParserException : Exception
	{
		public MoLangParserException(string message) : base(message) { }

		public MoLangParserException(string message, Exception innerException) : base(message, innerException) { }
	}
}