using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.Net.Bedrock;
using Alex.Worlds.Multiplayer.Bedrock;
using MiNET.Net;
using NLog;

namespace Alex.Services
{
	public class BedrockServerQueryProvider : IServerQueryProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockServerQueryProvider));
		
		private Alex Game { get; }
		public BedrockServerQueryProvider(Alex game)
		{
			Game = game;
		}

		/// <inheritdoc />
		public Task QueryServerAsync(ServerConnectionDetails connectionDetails,
			PingServerDelegate pingCallback,
			ServerStatusDelegate statusCallBack)
		{
		//	ManualResetEventSlim ar       = new ManualResetEventSlim(false);
			Stopwatch            sw       = new Stopwatch();
			long                 pingTime = 0;

			BedrockClient client = null;

			try
			{
				IPEndPoint serverEndpoint = new IPEndPoint(
					connectionDetails.EndPoint.Address, (int) connectionDetails.EndPoint.Port);

				client = new BedrockClient(
					Game, serverEndpoint,
					new PlayerProfile(
						string.Empty, $"Pinger{serverEndpoint.ToString()}", $"Pinger{serverEndpoint.ToString()}", null,
						null, null), null)
				{

					//IgnoreUnConnectedPong = true
				};

				client.Connection.AutoConnect = false;


				BedrockMotd motd = new BedrockMotd(string.Empty);

				client.OnMotdReceivedHandler += (sender, m) =>
				{
					motd = m;
					pingTime = m.Latency;
					//ar.Set();

					pingCallback.Invoke(new ServerPingResponse(true, pingTime));
				};
				
				sw.Restart();
				if (client.Start(TimeSpan.FromSeconds(30)))
				{
					client.Close();

					var compatability = CompatibilityResult.Unknown;

					if (motd.ProtocolVersion == McpeProtocolInfo.ProtocolVersion)
					{
						compatability = CompatibilityResult.Compatible;
					}

					statusCallBack?.Invoke(
						new ServerQueryResponse(
							true,
							new ServerQueryStatus()
							{
								EndPoint = motd.ServerEndpoint,
								Delay = pingTime,
								Success = true,
								Address = connectionDetails.Hostname,
								Port = (ushort) connectionDetails.EndPoint.Port,
								WaitingOnPing = false,
								Query = new ServerQuery()
								{
									Players = new Players() {Max = motd.MaxPlayers, Online = motd.Players},
									Version = new API.Services.Version()
									{
										Protocol = motd.ProtocolVersion,
										Name = motd.ClientVersion,
										Compatibility = compatability
									},
									Description = new Description() {Text = motd.MOTD},
									Modinfo = null,
									Favicon = null
								}
							}));
				}
				else
				{
					statusCallBack?.Invoke(
						new ServerQueryResponse(
							false, "multiplayer.status.cannot_connect",
							new ServerQueryStatus()
							{
								EndPoint = serverEndpoint,
								Delay = sw.ElapsedMilliseconds,
								Success = false,
								Address = connectionDetails.Hostname,
								Port = (ushort) connectionDetails.EndPoint.Port,
								WaitingOnPing = false
							}));
				}
			}
			catch (Exception e)
			{
				Log.Error($"Could not get bedrock query: {e.ToString()}");

				statusCallBack?.Invoke(
					new ServerQueryResponse(false, "Failed to connect...", new ServerQueryStatus() {Success = false}));
			}
			finally
			{
				client?.Close();
				client?.Dispose();
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public async Task StartLanDiscovery(CancellationToken cancellationToken, LandDiscoveryDelegate callback = null)
		{
			if (callback == null)
				return;
			
			Stopwatch        sw         = new Stopwatch();
			sw.Start();
			
			RaknetConnection connection = new RaknetConnection();
			connection.RemoteEndpoint = null;
			connection.AutoConnect = false;
			connection.Start();

			while (!connection.FoundServer)
			{
				if ((!sw.IsRunning || sw.ElapsedMilliseconds > 100))
				{
					sw.Restart();
					connection.SendUnconnectedPingInternal(null);
				}

				if (cancellationToken.IsCancellationRequested || connection.FoundServer)
					break;

				await Task.Delay(200);
			}

			sw.Stop();

			var remoteEndPoint   = connection.RemoteEndpoint;
			var remoteServerName = connection.RemoteServerName;
			var ping             = sw.ElapsedMilliseconds;

			connection.Session?.Close();
			connection.Stop();
			
			var motd = new BedrockMotd(remoteServerName);

			await callback.Invoke(
				new LanDiscoveryResult(
					remoteEndPoint, new ServerPingResponse(connection.FoundServer, ping),
					new ServerQueryResponse(
						connection.FoundServer,
						new ServerQueryStatus()
						{
							Address = remoteEndPoint.Address.ToString(),
							Delay = sw.ElapsedMilliseconds,
							Port = (ushort) remoteEndPoint.Port,
							Query = new ServerQuery()
							{
								Players = new Players() {Max = motd.MaxPlayers, Online = motd.Players},
								Version = new API.Services.Version()
								{
									Protocol = motd.ProtocolVersion,
									Name = motd.ClientVersion,
									Compatibility = CompatibilityResult.Compatible
								},
								Description = new Description() {Text = motd.MOTD},
								Modinfo = null,
								Favicon = null
							},
							WaitingOnPing = false,
							Success = true,
							EndPoint = remoteEndPoint
						})));
		}
	}
}