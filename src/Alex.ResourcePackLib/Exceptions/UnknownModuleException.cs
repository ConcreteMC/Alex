using System;

namespace Alex.ResourcePackLib.Exceptions
{
	public class UnknownModuleException : Exception
	{
		public UnknownModuleException(string message) : base(message) { }
	}
}