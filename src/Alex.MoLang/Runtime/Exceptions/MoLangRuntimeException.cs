using System;

namespace Alex.MoLang.Runtime.Exceptions
{
	public class MoLangRuntimeException : Exception
	{
		public MoLangRuntimeException(string message, Exception baseException) : base(message, baseException)
		{
			
		}
	}
}