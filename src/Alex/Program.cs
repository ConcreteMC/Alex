using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Alex.API.Services;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Handshake;
using Alex.Networking.Java.Packets.Status;
using Alex.Services;
using Newtonsoft.Json;
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
		//	Test();
		//	Console.ReadLine();
		//	return;
			LaunchSettings launchSettings = new LaunchSettings();
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
							launchSettings.Server = new IPEndPoint(val, reee);
						}
					}
				}

				if (nextIsaccessToken)
				{
					nextIsaccessToken = false;
					launchSettings.AccesToken = arg;
				}

				if (nextIsuuid)
				{
					nextIsuuid = false;
					launchSettings.UUID = arg;
				}

				if (nextIsUsername)
				{
					nextIsUsername = false;
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
			}

			if (launchSettings.Server == null && launchSettings.ConnectOnLaunch)
			{
				launchSettings.ConnectOnLaunch = false;
				Log.Warn($"No server specified, ignoring connect argument.");
			}

			Log.Info($"Starting...");
			using (var game = new Alex(launchSettings))
			{
				game.Run();
			}

		}

		private static void Test()
		{
			MCPacketFactory.Load();
			Stopwatch sw = Stopwatch.StartNew();

			string hostname = "127.0.0.1";
			ushort port = 25565;

			NetConnection conn;
			var client = new TcpClient();

			client.Connect(hostname, port);
			var endPoint = client.Client.RemoteEndPoint as IPEndPoint;

			if (client.Connected)
			{
				conn = new NetConnection(Direction.ClientBound, client.Client);
				//using (var conn = new NetConnection(Direction.ClientBound, client.Client))
				{
					AutoResetEvent ar = new AutoResetEvent(false);
					string jsonResponse = null;

					conn.OnPacketReceived += (sender, args) =>
					{
						if (args.Packet is ResponsePacket responsePacket)
						{
							jsonResponse = responsePacket.ResponseMsg;
							ar.Set();
						}
					};

					conn.OnConnectionClosed += (sender, args) => { ar.Set(); };

					conn.Initialize();

					conn.SendPacket(new HandshakePacket()
					{
						NextState = ConnectionState.Status,
						ServerAddress = hostname,
						ServerPort = port,
						ProtocolVersion = JavaProtocol.ProtocolVersion
					});

					conn.ConnectionState = ConnectionState.Status;

					conn.SendPacket(new RequestPacket());

					if (ar.WaitOne(10000) && jsonResponse != null)
					{
						var json = JsonConvert.DeserializeObject<ServerQueryProvider.ServerListPingJson>(jsonResponse);
						Console.WriteLine("Got result: " + json);

					/*	return new ServerQueryResponse(true, new ServerQueryStatus()
						{
							Delay = sw.ElapsedMilliseconds,
							Success = true,

							EndPoint = endPoint,
							Address = hostname,
							Port = port,

							Motd = json.Description?.Text,
							ProtocolVersion = json.Version?.Protocol ?? -1,
							Version = json.Version?.Name ?? string.Empty,
							NumberOfPlayers = json.Players?.Online ?? -1,
							MaxNumberOfPlayers = json.Players?.Max ?? -1,
							FaviconDataRaw = json.Favicon
						});*/
					}
				}
			}
		}
	}
#endif

	public class LaunchSettings
	{
		public bool ConnectOnLaunch = false;
		public IPEndPoint Server = null;

		public string Username;
		public string UUID;
		public string AccesToken;
	}
}
