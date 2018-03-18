using System;
using System.IO;
using System.Reflection;
using NLog;

namespace Alex
{
#if WINDOWS || LINUX
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Program));

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Log.Info($"Starting...");
			using (var game = new Alex())
			{
				game.Run();
			}

		}
	}
#endif
}
