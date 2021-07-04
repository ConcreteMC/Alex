using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Utils;
using Alex.Utils;
using Alex.Utils.Auth;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using NLog;
using NLog.Config;
using RocketUI.Utilities.IO;
using LogManager = NLog.LogManager;

namespace Alex
{
	/// <summary>
	/// The main class.
	/// </summary>
	public static class Program
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Program));
		private static Thread _startupThread = null;
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);
			_startupThread = Thread.CurrentThread;
			_startupThread.Name = "UI Thread";
			
		//	Test();
		//	Console.ReadLine();
			
		//	return;
		//	return;
			
			var argsResult = Parser.Default.ParseArguments<LaunchSettings>(args)
				.WithParsed(LaunchGame)
				;//.WithNotParsed()	
			//launchSettings = ParseArguments(args);

		}
		
		private static void LaunchGame(LaunchSettings launchSettings)
		{
			LoggerSetup.ConfigureNLog(launchSettings.WorkDir, Resources.NLogConfig);

			if (launchSettings.Server == null && launchSettings.ConnectOnLaunch)
			{
				launchSettings.ConnectOnLaunch = false;
				Log.Warn($"No server specified, ignoring connect argument.");
			}

            if (!Clipboard.IsClipboardAvailable())
            {
	            Log.Warn($"No suitable Clipboard implementation, clipboard will not be available! If you are on linux, install 'XClip' using 'apt install XClip'");
            }

			//Cef.Initialize(new Settings());

			Log.Info($"Starting...");

			using (var game = new Alex(launchSettings))
			{
				game.Run();
			}
		}
		
		public static bool IsRunningOnStartupThread()
		{
			return Thread.CurrentThread == _startupThread;
		}
	}

}
