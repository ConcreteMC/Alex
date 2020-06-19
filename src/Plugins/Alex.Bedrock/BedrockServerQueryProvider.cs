using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.Bedrock.Worlds;
using Alex.Services;
using Alex.Utils;
using MiNET.Utils;
using NLog;

namespace Alex.Bedrock
{
	public class BedrockServerQueryProvider : IServerQueryProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockServerQueryProvider));
		
		private BedrockPlugin Plugin { get; }
		public BedrockServerQueryProvider(BedrockPlugin plugin)
		{
			Plugin = plugin;
		}
		
		/// <inheritdoc />
		public async Task QueryServerAsync(ServerConnectionDetails connectionDetails,
		    PingServerDelegate pingCallback, ServerStatusDelegate statusCallBack)
		{
			ManualResetEventSlim ar = new ManualResetEventSlim(false);
			Stopwatch sw = new Stopwatch();
		    long pingTime = 0;

			/*var result = await ResolveHostnameAsync(hostname);
			if (!result.Success)
			{
				statusCallBack?.Invoke(new ServerQueryResponse(false, "multiplayer.status.cannot_resolve", new ServerQueryStatus()
				{
					Delay = sw.ElapsedMilliseconds,
					Success = false,

					EndPoint = null,
					Address = hostname,
					Port = port
				}));

				return;
			}*/

			using(DedicatedThreadPool threadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(1, ThreadType.Background, "ServerPingThread")))
		    {
			    BedrockClient client = null;
			    try
			    {
					IPEndPoint serverEndpoint = new IPEndPoint(connectionDetails.EndPoint.Address, (int) connectionDetails.EndPoint.Port);

					client = new BedrockClient(Plugin, Plugin.Game, null, serverEndpoint,
						new PlayerProfile(string.Empty, $"Pinger{serverEndpoint.ToString()}",
							$"Pinger{serverEndpoint.ToString()}", null, null, null, "xbox"), threadPool, null)
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
				    //client.SendUnconnectedPing();
				    sw.Restart();

					//ar.WaitAsync().Wait(TimeSpan.FromMilliseconds(10000));

				    if (await WaitHandleHelpers.FromWaitHandle(ar.WaitHandle, TimeSpan.FromMilliseconds(10000)))
				    {
					    client.Close();
					  //  var m = new BedrockMotd(client.Connection.RemoteServerName);

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
				    client?.Dispose();
				}
		    }
	    }
	}
}