using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;

namespace Alex.CoreRT
{
#if WINDOWS || LINUX
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
			XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

			using (var game = new Alex())
                game.Run();

		}
	}
#endif
}
