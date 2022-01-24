using System;
using System.IO;
using System.Net;
using CommandLine;

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

		[Option(
			"rocket-debug", Default = false, Required = false,
			HelpText = "Adds the required services for the RocketUI designer to work")]
		public bool RocketDebugging { get; set; } = false;

		public LaunchSettings()
		{
			var appData = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);

			WorkDir = Path.Combine(appData, "Alex");
		}
	}
}