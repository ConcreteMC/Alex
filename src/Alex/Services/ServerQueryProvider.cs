using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Handshake;
using Alex.Networking.Java.Packets.Status;
using Alex.Utils;
using Alex.Worlds.Bedrock;
using MiNET.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.Services
{
    public class ServerQueryProvider : IServerQueryProvider
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ServerQueryProvider));
		private static FastRandom Rnd = new FastRandom();
		
		private Alex Alex { get; }
        public ServerQueryProvider(Alex alex)
        {
	        Alex = alex;
            MCPacketFactory.Load();
        }

	    private static bool ResolveHostname(string hostname, out IPAddress address)
	    {
		    IPAddress[] ipAddresses = Dns.GetHostAddresses(hostname);
		    if (ipAddresses.Length <= 0)
		    {
			    address = default(IPAddress);
			    return false;
		    }

		    address = ipAddresses[Rnd.Next(0, ipAddresses.Length - 1)];
		    return true;
	    }

	    private class ResolveResult
	    {
		    public bool Success;
		    public IPAddress Result;

		    public ResolveResult(bool success, IPAddress result)
		    {
			    Success = success;
			    Result = result;
		    }
	    }

	    private static async Task<ResolveResult> ResolveHostnameAsync(string hostname)
	    {
		    IPAddress[] ipAddresses = await Dns.GetHostAddressesAsync(hostname);
		    if (ipAddresses.Length <= 0)
		    {
			    return new ResolveResult(false, default(IPAddress));
		    }

		    return new ResolveResult(true, ipAddresses[Rnd.Next(0, ipAddresses.Length - 1)]);
	    }

		public async Task QueryBedrockServerAsync(string hostname, ushort port, PingServerDelegate pingCallback, ServerStatusDelegate statusCallBack)
	    {
		    await QueryPeServerAsync(hostname, port, pingCallback, statusCallBack);
	    }

		private async Task QueryPeServerAsync(string hostname, ushort port,
		    PingServerDelegate pingCallback, ServerStatusDelegate statusCallBack)
		{
			ManualResetEventSlim ar = new ManualResetEventSlim(false);
			Stopwatch sw = new Stopwatch();
		    long pingTime = 0;

			var result = await ResolveHostnameAsync(hostname);
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
			}

			using(DedicatedThreadPool threadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(1, ThreadType.Background, "ServerPingThread")))
		    {
			    BedrockClient client = null;
			    try
			    {
					IPEndPoint serverEndpoint = new IPEndPoint(result.Result, (int) port);

					client = new BedrockClient(Alex, null, serverEndpoint,
						new PlayerProfile(string.Empty, $"Pinger{serverEndpoint.ToString()}",
							$"Pinger{serverEndpoint.ToString()}", null, null, null, true), threadPool, null)
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

						    Address = hostname,
						    Port = port,
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

							    Address = hostname,
							    Port = port,
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


	    public async Task QueryServerAsync(string hostname, ushort port, PingServerDelegate pingCallback, ServerStatusDelegate statusCallBack)
        {
			await QueryJavaServerAsync(hostname, port, pingCallback, statusCallBack);
        }

        private static async Task QueryJavaServerAsync(string hostname, ushort port, PingServerDelegate pingCallback, ServerStatusDelegate statusCallBack)
        {
			IPEndPoint endPoint = null;
	        var sw = Stopwatch.StartNew();
	        string jsonResponse = null;
			try
			{
				var result = await ResolveHostnameAsync(hostname);
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
				}

				bool waitingOnPing = true;
				using (TcpClient client = new TcpClient())
		        {
			        await client.ConnectAsync(result.Result, port);
			        endPoint = client.Client.RemoteEndPoint as IPEndPoint;

			        if (client.Connected)
			        {
				        //conn = new NetConnection(Direction.ClientBound, client.Client);
				        //conn.LogExceptions = false;
				        using (var conn = new NetConnection(Direction.ClientBound, client.Client)
				        {
					        LogExceptions = true
				        })
				        {
					        long pingId = Rnd.NextUInt();

							EventWaitHandle ar = new EventWaitHandle(false, EventResetMode.AutoReset);

					        conn.OnPacketReceived += (sender, args) =>
					        {
						        if (args.Packet is ResponsePacket responsePacket)
						        {
							        jsonResponse = responsePacket.ResponseMsg;
							        ar.Set();
						        }
						        else if (args.Packet is PingPacket pong)
						        {
							        if (pong.Payload == pingId)
							        {
								        waitingOnPing = false;
                                        pingCallback?.Invoke(new ServerPingResponse(true, sw.ElapsedMilliseconds));
							        }
							        else
							        {
								        waitingOnPing = false;
                                        pingCallback?.Invoke(new ServerPingResponse(true, sw.ElapsedMilliseconds));
									}
						        }
					        };

					        bool connectionClosed = false;
					        conn.OnConnectionClosed += (sender, args) =>
					        {
						        connectionClosed = true;
						        ar.Set();
					        };

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

					        if (await WaitHandleHelpers.FromWaitHandle(ar, TimeSpan.FromMilliseconds(10000)) &&
					            !connectionClosed && jsonResponse != null)
					        {

						        long timeElapsed = sw.ElapsedMilliseconds;
						      //  Log.Debug($"Server json: " + jsonResponse);
								var query = ServerQuery.FromJson(jsonResponse);

						        if (pingCallback != null)
						        {
									conn.SendPacket(new PingPacket()
							        {
								        Payload = pingId,
							        });

							        sw.Restart();
								}

						        var r = new ServerQueryStatus()
						        {
							        Delay = timeElapsed,
							        Success = true,
							        WaitingOnPing = pingCallback != null && !waitingOnPing,

							        EndPoint = endPoint,
							        Address = hostname,
							        Port = port,

									Query = query
						        };

								statusCallBack?.Invoke(new ServerQueryResponse(true, r));
					        }
					        else
					        {
						        statusCallBack?.Invoke(new ServerQueryResponse(false, "multiplayer.status.cannot_connect",
							        new ServerQueryStatus()
							        {
								        EndPoint = endPoint,
								        Delay = sw.ElapsedMilliseconds,
								        Success = false,
								        /* Motd = motd.MOTD,
								         ProtocolVersion = motd.ProtocolVersion,
								         MaxNumberOfPlayers = motd.MaxPlayers,
								         Version = motd.ClientVersion,
								         NumberOfPlayers = motd.Players,*/

								        Address = hostname,
								        Port = port,
								        WaitingOnPing = false

							        }));
					        }
				        }
			        }
		        }
	        }
	        catch (Exception ex)
	        {
		        if (sw.IsRunning)
			        sw.Stop();

		        Log.Error($"Could not get server query result! Exception: {ex.ToString()}");

		        string msg = ex.Message;
		        if (ex is SocketException)
		        {
			        msg = $"multiplayer.status.cannot_connect";
		        }

		        statusCallBack?.Invoke(new ServerQueryResponse(false, msg, new ServerQueryStatus()
		        {
			        Delay = sw.ElapsedMilliseconds,
			        Success = false,

			        EndPoint = endPoint,
			        Address = hostname,
			        Port = port
		        }));
	        }
	        finally
	        {
		        //conn?.Stop();
				//conn?.Dispose();
				//conn?.Dispose();
			//	client?.Close();
	        }
        }

	    public class ServerListPingJson
        {
            public ServerListPingVersionJson Version { get; set; } = new ServerListPingVersionJson();
            public ServerListPingPlayersJson Players { get; set; } = new ServerListPingPlayersJson();

			[JsonConverter(typeof(ServerListPingDescriptionJson.DescriptionConverter))]
            public ServerListPingDescriptionJson Description { get; set; } = new ServerListPingDescriptionJson();
            public string Favicon { get; set; }
        }

        public class ServerListPingVersionJson
        {
            public string Name { get; set; }
            public int Protocol { get; set; }
        }

	    public class ServerListPingPlayersJson
        {
            public int Max { get; set; }
            public int Online { get;set; }
        }

	    public class ServerListPingDescriptionJson
        {
            public string Text { get; set; }

	        public class DescriptionConverter : JsonConverter<ServerListPingDescriptionJson>
	        {
		        public override ServerListPingDescriptionJson ReadJson(JsonReader reader, Type objectType, ServerListPingDescriptionJson existingValue,
			        bool hasExistingValue, JsonSerializer serializer)
		        {
					if (reader.TokenType == JsonToken.StartObject)
			        {
				        JObject item = JObject.Load(reader);
				        return item.ToObject<ServerListPingDescriptionJson>();
			        }
					else if (reader.TokenType == JsonToken.String)
					{
						return new ServerListPingDescriptionJson()
						{
							Text = (string) reader.Value
						};
					}

			        return null;
		        }

		        public override bool CanWrite
		        {
			        get { return false; }
		        }

		        public override void WriteJson(JsonWriter writer, ServerListPingDescriptionJson value, JsonSerializer serializer)
		        {
			        throw new NotImplementedException();
		        }
	        }
		}

        private async Task<ServerQueryResponse> QueryLegacyServerAsync(string hostname, ushort port)
        {
            var sw = Stopwatch.StartNew();
            IPEndPoint endPoint = null;
            try
            {
                byte[] buffer = new byte[1024];

                using (var client = new TcpClient())
                {

                    await client.ConnectAsync(hostname, port);
                    sw.Stop();

                    endPoint = client.Client.RemoteEndPoint as IPEndPoint;
                    
                    using (var ns = client.GetStream())
                    {
                        var payload = new byte[] {0xFE, 0x01};
                        await ns.WriteAsync(payload, 0, payload.Length);


                        await ns.ReadAsync(buffer, 0, buffer.Length);
                    }

                    client.Close();

                    var serverData = Encoding.Unicode.GetString(buffer).Split("\u0000\u0000\u0000".ToCharArray());
                    if (serverData.Length >= 6)
                    {
                        var status = new ServerQueryStatus()
                        {
                            Delay   = sw.ElapsedMilliseconds,
                            Success = true,

							Query = new ServerQuery()
							{
								Version = new API.Services.Version()
								{
									Name = serverData[2]
								},
								Players = new Players() { Online = int.Parse(serverData[4]), Max = int.Parse(serverData[5]) },
								Description = new Description()
								{
									Text = serverData[3]
								}
							},

                            EndPoint = endPoint,
                            Address  = hostname,
                            Port     = port
                        };

                        return new ServerQueryResponse(true, status);
                    }
                }
            }
            catch (Exception ex)
            {
                if(sw.IsRunning)
                    sw.Stop();

                return new ServerQueryResponse(false, ex.Message, new ServerQueryStatus()
                {
                    Delay = sw.ElapsedMilliseconds,
                    Success = false,

                    EndPoint = endPoint,
                    Address = hostname,
                    Port = port
                });
            }
            
            if(sw.IsRunning)
                sw.Stop();

            return new ServerQueryResponse(false, "Unknown Error", new ServerQueryStatus()
            {
                Delay   = sw.ElapsedMilliseconds,
                Success = false,
                
                EndPoint = endPoint,
                Address  = hostname,
                Port     = port
            });
        }
    }

	public static class WaitHandleHelpers{
		/// <summary>
		/// Wraps a <see cref="WaitHandle"/> with a <see cref="Task"/>. When the <see cref="WaitHandle"/> is signalled, the returned <see cref="Task"/> is completed. If the handle is already signalled, this method acts synchronously.
		/// </summary>
		/// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
		public static Task FromWaitHandle(WaitHandle handle)
		{
			return FromWaitHandle(handle, Timeout.InfiniteTimeSpan, CancellationToken.None);
		}

		/// <summary>
		/// Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. If the <see cref="WaitHandle"/> is signalled, the returned task is completed with a <c>true</c> result. If the observation times out, the returned task is completed with a <c>false</c> result. If the handle is already signalled or the timeout is zero, this method acts synchronously.
		/// </summary>
		/// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
		/// <param name="timeout">The timeout after which the <see cref="WaitHandle"/> is no longer observed.</param>
		public static Task<bool> FromWaitHandle(WaitHandle handle, TimeSpan timeout)
		{
			return FromWaitHandle(handle, timeout, CancellationToken.None);
		}

		/// <summary>
		/// Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. If the <see cref="WaitHandle"/> is signalled, the returned task is (successfully) completed. If the observation is cancelled, the returned task is cancelled. If the handle is already signalled or the cancellation token is already cancelled, this method acts synchronously.
		/// </summary>
		/// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
		/// <param name="token">The cancellation token that cancels observing the <see cref="WaitHandle"/>.</param>
		public static Task FromWaitHandle(WaitHandle handle, CancellationToken token)
		{
			return FromWaitHandle(handle, Timeout.InfiniteTimeSpan, token);
		}

		/// <summary>
		/// Wraps a <see cref="WaitHandle"/> with a <see cref="Task{Boolean}"/>. If the <see cref="WaitHandle"/> is signalled, the returned task is completed with a <c>true</c> result. If the observation times out, the returned task is completed with a <c>false</c> result. If the observation is cancelled, the returned task is cancelled. If the handle is already signalled, the timeout is zero, or the cancellation token is already cancelled, then this method acts synchronously.
		/// </summary>
		/// <param name="handle">The <see cref="WaitHandle"/> to observe.</param>
		/// <param name="timeout">The timeout after which the <see cref="WaitHandle"/> is no longer observed.</param>
		/// <param name="token">The cancellation token that cancels observing the <see cref="WaitHandle"/>.</param>
		public static Task<bool> FromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken token)
		{
			// Handle synchronous cases.
			var alreadySignalled = handle.WaitOne(0);
			if (alreadySignalled)
				return TaskConstants.BooleanTrue;
			if (timeout == TimeSpan.Zero)
				return TaskConstants.BooleanFalse;
			if (token.IsCancellationRequested)
				return TaskConstants<bool>.Canceled;

			// Register all asynchronous cases.
			return DoFromWaitHandle(handle, timeout, token);
		}

		private static async Task<bool> DoFromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken token)
		{
			var tcs = new TaskCompletionSource<bool>();
			using (new ThreadPoolRegistration(handle, timeout, tcs))
			using (token.Register(state => ((TaskCompletionSource<bool>)state).TrySetCanceled(), tcs, useSynchronizationContext: false))
				return await tcs.Task.ConfigureAwait(false);
		}

		private sealed class ThreadPoolRegistration : IDisposable
		{
			private readonly RegisteredWaitHandle _registeredWaitHandle;

			public ThreadPoolRegistration(WaitHandle handle, TimeSpan timeout, TaskCompletionSource<bool> tcs)
			{
				_registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(handle,
					(state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut), tcs,
					timeout, executeOnlyOnce: true);
			}

			void IDisposable.Dispose() => _registeredWaitHandle.Unregister(null);
		}
	}

	public static class TaskConstants
	{
		private static readonly Task<bool> booleanTrue = Task.FromResult(true);
		private static readonly Task<int> intNegativeOne = Task.FromResult(-1);

		/// <summary>
		/// A task that has been completed with the value <c>true</c>.
		/// </summary>
		public static Task<bool> BooleanTrue
		{
			get
			{
				return booleanTrue;
			}
		}

		/// <summary>
		/// A task that has been completed with the value <c>false</c>.
		/// </summary>
		public static Task<bool> BooleanFalse
		{
			get
			{
				return TaskConstants<bool>.Default;
			}
		}

		/// <summary>
		/// A task that has been completed with the value <c>0</c>.
		/// </summary>
		public static Task<int> Int32Zero
		{
			get
			{
				return TaskConstants<int>.Default;
			}
		}

		/// <summary>
		/// A task that has been completed with the value <c>-1</c>.
		/// </summary>
		public static Task<int> Int32NegativeOne
		{
			get
			{
				return intNegativeOne;
			}
		}

		/// <summary>
		/// A <see cref="Task"/> that has been completed.
		/// </summary>
		public static Task Completed
		{
			get
			{
				return Task.CompletedTask;
			}
		}

		/// <summary>
		/// A task that has been canceled.
		/// </summary>
		public static Task Canceled
		{
			get
			{
				return TaskConstants<object>.Canceled;
			}
		}
	}

	/// <summary>
	/// Provides completed task constants.
	/// </summary>
	/// <typeparam name="T">The type of the task result.</typeparam>
	public static class TaskConstants<T>
	{
		private static readonly Task<T> defaultValue = Task.FromResult(default(T));
		private static readonly Task<T> canceled = Task.FromCanceled<T>(new CancellationToken(true));

		/// <summary>
		/// A task that has been completed with the default value of <typeparamref name="T"/>.
		/// </summary>
		public static Task<T> Default
		{
			get
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// A task that has been canceled.
		/// </summary>
		public static Task<T> Canceled
		{
			get
			{
				return canceled;
			}
		}
	}
}
