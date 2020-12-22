using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Input;
using Alex.API.Services;
using Alex.Utils;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
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
			_startupThread = Thread.CurrentThread;
			_startupThread.Name = "UI Thread";
			
			Test();
			Console.ReadLine();
			
			return;
			
			var argsResult = Parser.Default.ParseArguments<LaunchSettings>(args)
				.WithParsed(LaunchGame)
				;//.WithNotParsed()	
			//launchSettings = ParseArguments(args);

		}

		private static async void Test()
		{
			XboxAuthService authService = new XboxAuthService();
			var             authConnect = await authService.StartDeviceAuthConnect();
			
			Console.WriteLine($"Code: {authConnect.user_code}");
			
			XboxAuthService.OpenBrowser(authConnect.verification_uri);
			
			await authService.DoDeviceCodeLogin("", authConnect.device_code, CancellationToken.None);
		}

		private static void LaunchGame(LaunchSettings launchSettings)
		{
			if (!Directory.Exists(launchSettings.WorkDir))
			{
				Directory.CreateDirectory(launchSettings.WorkDir);
			}

			ConfigureNLog(launchSettings.WorkDir);

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

		private static void ConfigureNLog(string baseDir)
		{
			string loggerConfigFile = Path.Combine(baseDir, "NLog.config");
			if (!File.Exists(loggerConfigFile))
			{
				File.WriteAllText(loggerConfigFile, Resources.NLogConfig);
			}

			string logsDir = Path.Combine(baseDir, "logs");
			if (!Directory.Exists(logsDir))
			{
				Directory.CreateDirectory(logsDir);
			}

			LogManager.ThrowConfigExceptions = false;
			LogManager.LoadConfiguration(loggerConfigFile);
//			LogManager.Configuration = new XmlLoggingConfiguration(loggerConfigFile);
			LogManager.Configuration.Variables["basedir"] = baseDir;

			NLogAppender.Initialize();
        }
	}

}
