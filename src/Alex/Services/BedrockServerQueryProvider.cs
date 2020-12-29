using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Services;
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
		public async Task QueryServerAsync(ServerConnectionDetails connectionDetails,
		    PingServerDelegate pingCallback, ServerStatusDelegate statusCallBack)
		{
			ManualResetEventSlim    ar       = new ManualResetEventSlim(false);
			Stopwatch               sw       = new Stopwatch();
		    long                    pingTime = 0;

		    BedrockClient client = null;
		    using(MiNET.Utils.DedicatedThreadPool threadPool = new MiNET.Utils.DedicatedThreadPool(new MiNET.Utils.DedicatedThreadPoolSettings(1, MiNET.Utils.ThreadType.Background, "ServerPingThread")))
		    {
			    try
			    {
					IPEndPoint serverEndpoint = new IPEndPoint(connectionDetails.EndPoint.Address, (int) connectionDetails.EndPoint.Port);

					client = new BedrockClient(Game, serverEndpoint,
						new PlayerProfile(string.Empty, $"Pinger{serverEndpoint.ToString()}",
							$"Pinger{serverEndpoint.ToString()}", null, null, null), threadPool, null)
					{
						
						//IgnoreUnConnectedPong = true
					};
					client.Connection.AutoConnect = false;
					

				    BedrockMotd motd = new BedrockMotd(string.Empty);

					client.OnMotdReceivedHandler += (sender, m) =>
				    {
						motd = m;
						pingTime = m.Latency;
					    ar.Set();

						pingCallback.Invoke(new ServerPingResponse(true, pingTime));
				    };

				    client.Start(ar);

				    sw.Restart();

				    if (await WaitHandleHelpers.FromWaitHandle(ar.WaitHandle, TimeSpan.FromMilliseconds(2500)))
				    {
					    client.Close();

					  var compatability = CompatibilityResult.Unknown;

					  if (motd.ProtocolVersion == McpeProtocolInfo.ProtocolVersion)
					  {
						  compatability = CompatibilityResult.Compatible;
					  }

					  statusCallBack?.Invoke(new ServerQueryResponse(true, new ServerQueryStatus()
					    {
						    EndPoint = motd.ServerEndpoint,
						    Delay = pingTime,
						    Success = true,

						    Address = connectionDetails.Hostname,
						    Port = (ushort) connectionDetails.EndPoint.Port,
						    WaitingOnPing = false,
						    
							Query = new ServerQuery()
							{
								Players = new Players()
								{
									Max = motd.MaxPlayers,
									Online = motd.Players
								},
								Version = new API.Services.Version()
								{
									Protocol = motd.ProtocolVersion,
									Name = motd.ClientVersion,
									Compatibility = compatability
								},
								Description = new Description()
								{
									Text = motd.MOTD
								},
								Modinfo = null,
								Favicon = null
							}
					    }));
				    }
				    else
				    {
					    statusCallBack?.Invoke(new ServerQueryResponse(false, "multiplayer.status.cannot_connect",
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
				    statusCallBack?.Invoke(new ServerQueryResponse(false, "Failed to connect...", new ServerQueryStatus()
				    {
						Success = false
				    }));
			    }
			    finally
			    {
				    client?.Close();
				    client?.Dispose();
				}
		    }
	    }
	}
}