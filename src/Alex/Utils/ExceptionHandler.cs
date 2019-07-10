using System;

namespace Alex.Utils
{
	public static class ExceptionHandler
	{

		public static void Initialize()
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
		}

		private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{

		}
	}
}
