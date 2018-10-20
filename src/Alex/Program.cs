using System;
using System.Net;
using Alex.API.Data.Chat;
using Alex.API.Data.Chat.Serializer;
using CefSharp;
using Chromely.CefGlue.Winapi;
using Chromely.CefGlue.Winapi.ChromeHost;
using Chromely.Core;
using Chromely.Core.Helpers;
using log4net;
using Newtonsoft.Json;
using NLog;
using WinApi.Windows;
using LogManager = NLog.LogManager;

namespace Alex
{
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
		static void Main(string[] args)
		{
			NLogAppender.Initialize();
			LaunchSettings launchSettings = ParseArguments(args);

			if (launchSettings.Server == null && launchSettings.ConnectOnLaunch)
			{
				launchSettings.ConnectOnLaunch = false;
				Log.Warn($"No server specified, ignoring connect argument.");
			}

			//Cef.Initialize(new Settings());

			Log.Info($"Starting...");
			using (var game = new Alex(launchSettings))
			{
				game.Run();
			}

		}


		private class Settings : AbstractCefSettings
		{

		}

		private static LaunchSettings ParseArguments(string[] args)
		{
			LaunchSettings launchSettings    = new LaunchSettings();
			bool           nextIsServer      = false;
			bool           nextIsuuid        = false;
			bool           nextIsaccessToken = false;
			bool           nextIsUsername    = false;
			
			foreach (var arg in args)
			{
				if (nextIsServer)
				{
					nextIsServer = false;
					var s = arg.Split(':');
					if (IPAddress.TryParse(s[0], out IPAddress val))
					{
						if (ushort.TryParse(s[1], out ushort reee))
						{
							launchSettings.Server = new IPEndPoint(val, reee);
						}
					}
				}

				if (nextIsaccessToken)
				{
					nextIsaccessToken         = false;
					launchSettings.AccesToken = arg;
				}

				if (nextIsuuid)
				{
					nextIsuuid          = false;
					launchSettings.UUID = arg;
				}

				if (nextIsUsername)
				{
					nextIsUsername          = false;
					launchSettings.Username = arg;
				}

				if (arg == "--server")
				{
					nextIsServer = true;
				}

				if (arg == "--accessToken")
				{
					nextIsaccessToken = true;
				}

				if (arg == "--uuid")
				{
					nextIsuuid = true;
				}

				if (arg == "--username")
				{
					nextIsUsername = true;
				}

				if (arg == "--direct")
				{
					launchSettings.ConnectOnLaunch = true;
				}

				if (arg == "--console")
				{
					launchSettings.ShowConsole = true;
				}
			}

			return launchSettings;
		}
		
	}

	public class LaunchSettings
	{
		public bool ConnectOnLaunch = false;
		public IPEndPoint Server = null;

		public string Username;
		public string UUID;
		public string AccesToken;
		public bool ShowConsole = false;
	}
}
