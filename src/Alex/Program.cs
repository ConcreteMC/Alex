using System;
using System.IO;
using System.Net;
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
		static void Main(string[] args)
		{
			bool nextIsServer = false;
			bool nextIsuuid = false;
			bool nextIsaccessToken = false;
			bool nextIsUsername = false;

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
							Alex.ServerEndPoint = new IPEndPoint(val, reee);
							Alex.IsMultiplayer = true;
						}
					}
				}

				if (nextIsaccessToken)
				{
					nextIsaccessToken = false;
					Alex.AccessToken = arg;
				}

				if (nextIsuuid)
				{
					nextIsuuid = false;
					Alex.UUID = arg;
				}

				if (nextIsUsername)
				{
					nextIsUsername = false;
					Alex.Username = arg;
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
			}


			Log.Info($"Starting...");
			using (var game = new Alex())
			{
				game.Run();
			}

		}
	}
#endif
}
