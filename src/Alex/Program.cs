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
using log4net;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;
using LogManager = NLog.LogManager;

namespace Alex
{
	public class LaunchSettings
	{
		[Option("direct", Default = false, Required = false, HelpText = "Connect to a server immediately on launch")]
		public bool ConnectOnLaunch { get; set; } = false;
		
		[Option("server", Default = null, Required = false, HelpText = "The serverIp:Port to connect to on launch")]
		public string TargetServer { get; set; } = null;
		public IPEndPoint Server
		{
			get
			{
				return IPEndPoint.TryParse(TargetServer, out var ep) ? ep : null;
			}
		}

		[Option('u', "username", Required = false, HelpText = "Override Player's Username")]
		public string Username { get; set; }
		
		[Option("uuid", Required = false, HelpText = "Override Player's UUID")]
		public string UUID { get; set; }
		
		[Option("accessToken", Required = false, HelpText = "Override Player's Access Token")]
		public string AccesToken { get; set; }
		
		[Option("Console", Default = false, Required = false, HelpText = "Show console window")]
		public bool ShowConsole { get; set; } = false;
		
		[Option("workDir", Required = false, HelpText = "Base Alex Directory")]
		public string WorkDir { get; set; }
		
		[Option("bedrock", Default = false, Required = false, HelpText = "Connect to a bedrock server")]
		public bool ConnectToBedrock { get; set; } = false;
		
		[Option("debug", Default = false, Required = false, HelpText = "Enable Model Debugging mode")]
		public bool ModelDebugging { get; set; } = false;
		
		public LaunchSettings()
		{
			var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
			WorkDir = Path.Combine(appData, "Alex");
		}
	}
	
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
			
			var argsResult = Parser.Default.ParseArguments<LaunchSettings>(args)
				.WithParsed(LaunchGame)
				;//.WithNotParsed()	
			//launchSettings = ParseArguments(args);

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
