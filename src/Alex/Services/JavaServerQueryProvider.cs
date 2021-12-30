using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Handshake;
using Alex.Networking.Java.Packets.Status;
using Alex.Utils;
using MiNET.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Alex.Services
{
    public class JavaServerQueryProvider : IServerQueryProvider
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaServerQueryProvider));
		private static FastRandom Rnd = new FastRandom();
		
		private Alex Alex { get; }
        public JavaServerQueryProvider(Alex alex)
        {
	        Alex = alex;
            MCPacketFactory.Load();
        }

	    public class ResolveResult
	    {
		    public bool Success;
		    public IPAddress[] Results;

		    public IPAddress Result => Results.Length == 0 ? null : Results[Rnd.Next() % Results.Length];
		    public ResolveResult(bool success, params IPAddress[] results)
		    {
			    Success = success;
			    Results = results;
		    }
	    }

	    public static async Task<ResolveResult> ResolveHostnameAsync(string hostname)
	    {
		    IPAddress[] ipAddresses = await Dns.GetHostAddressesAsync(hostname);
		    if (ipAddresses.Length <= 0)
		    {
			    return new ResolveResult(false, default(IPAddress));
		    }

		    return new ResolveResult(true, ipAddresses);
	    }

	    //
	    // \[(?<tag>.*)\](?<tagvalue>.*)\[\/(?P=tag)\]
	    public Task QueryServerAsync(ServerConnectionDetails connectionDetails, PingServerDelegate pingCallback, ServerStatusDelegate statusCallBack, CancellationToken cancellationToken)
        {
			return QueryJavaServerAsync(connectionDetails, pingCallback, statusCallBack, cancellationToken);
        }

	    private static Regex LanDiscoveryRegex = new Regex(@"\[(?<tag>.*)\](?<value>.*)\[\/(\k<tag>)\]");

	    /// <inheritdoc />
	    public Task StartLanDiscovery(CancellationToken cancellationToken, LandDiscoveryDelegate callback = null)
	    {
		    return Task.Run(
			    () =>
			    {
				    try
				    {
					    List<IPEndPoint> knownEndpoints = new List<IPEndPoint>();

					    using (UdpClient udp = new UdpClient(4445))
					    {
						    udp.EnableBroadcast = true;

						    udp.JoinMulticastGroup(IPAddress.Parse("224.0.2.60"));

						    cancellationToken.Register(
							    (o) =>
							    {
								    if (o != null && o is UdpClient client)
								    {
									    client?.Close();
								    }
							    }, udp);

						    while (!cancellationToken.IsCancellationRequested)
						    {
							    IPEndPoint? remoteEndPoint = null;

							    byte[] received = udp.Receive(ref remoteEndPoint);


							    if (!knownEndpoints.Contains(remoteEndPoint))
							    {
								    if (received.Length > 0)
								    {
									    knownEndpoints.Add(remoteEndPoint);
									    var message = Encoding.UTF8.GetString(received);
									    var match = LanDiscoveryRegex.Matches(message);

									    var port = remoteEndPoint.Port;
									    string motd = "";

									    foreach (Match m in match)
									    {
										    var key = m.Groups["tag"].Value;
										    var value = m.Groups["value"].Value;

										    switch (key)
										    {
											    case "MOTD":
												    motd = value;

												    break;

											    case "AD":
												    if (int.TryParse(value, out var p))
												    {
													    port = p;
												    }

												    break;
										    }
									    }

									    if (!string.IsNullOrWhiteSpace(motd))
									    {
										    callback?.Invoke(
											    new LanDiscoveryResult(
												    remoteEndPoint, new ServerPingResponse(true, 0),
												    new ServerQueryResponse(
													    true,
													    new ServerQueryStatus()
													    {
														    Delay = 0,
														    Success = true,
														    WaitingOnPing = false,
														    EndPoint = remoteEndPoint,
														    Address = remoteEndPoint.Address.ToString(),
														    Port = (ushort) port,
														    Query = new ServerQuery()
														    {
															    Description = new Description() {Text = motd}
														    }
													    })));
									    }
								    }
							    }
						    }
					    }
				    }catch(Exception){}
			    }, cancellationToken);

		    return Task.CompletedTask;
	    }

	    private class QueryPacketHandler : IPacketHandler
	    {
		    private ManualResetEventSlim _waitHandle;
		    private PingServerDelegate _callBack;
		    private NetConnection _connection;
		    private Action<string> _jsonCallback;

		    public QueryPacketHandler(NetConnection connection, ManualResetEventSlim eventWaitHandle, PingServerDelegate @delegate, Action<string> jsonCallback)
		    {
			    _connection = connection;
			    _waitHandle = eventWaitHandle;
			    _callBack = @delegate;
			    _jsonCallback = jsonCallback;
		    }

		    public long PingId { get; set; }

		    /// <inheritdoc />
		    public Task HandleHandshake(Packet packet)
		    {
			    throw new NotImplementedException();
		    }

		    private Stopwatch _sw = new Stopwatch();
		    /// <inheritdoc />
		    public Task HandleStatus(Packet packet)
		    {
			    if (packet is ResponsePacket responsePacket)
			    {
				    var jsonResponse = responsePacket.ResponseMsg;
				    _jsonCallback?.Invoke(jsonResponse);
				    
				    if (_callBack != null)
				    {
					    var ping = PingPacket.CreateObject();
					    ping.Payload = PingId;
					    _connection.SendPacket(ping);

					    _sw.Restart();
				    }
							        
				    _waitHandle.Set();
			    }
			    else if (packet is PingPacket pong)
			    {
				    var pingResult = _sw.ElapsedMilliseconds;
				    if (pong.Payload == PingId)
				    {
					    //waitingOnPing = false;
					    _callBack?.Invoke(new ServerPingResponse(true, _sw.ElapsedMilliseconds));
				    }
				    else
				    {
					    //waitingOnPing = false;
					    _callBack?.Invoke(new ServerPingResponse(true, _sw.ElapsedMilliseconds));
				    }

				    _waitHandle.Set();
			    }

			    return Task.CompletedTask;
		    }

		    /// <inheritdoc />
		    public Task HandleLogin(Packet packet)
		    {
			    throw new NotImplementedException();
		    }

		    /// <inheritdoc />
		    public Task HandlePlay(Packet packet)
		    {
			    throw new NotImplementedException();
		    }
	    }

	    private static async Task QueryJavaServerAsync(ServerConnectionDetails connectionDetails,
		    PingServerDelegate pingCallback,
		    ServerStatusDelegate statusCallBack,
		    CancellationToken cancellationToken)
	    {
		    //  CancellationTokenSource cts = new CancellationTokenSource(2500);
		    IPEndPoint endPoint = null;
		    var sw = Stopwatch.StartNew();
		    string jsonResponse = null;

		    try
		    {
			    bool waitingOnPing = true;

			    //conn = new NetConnection(Direction.ClientBound, client.Client);
			    //conn.LogExceptions = false;
			    using (var conn = new NetConnection(connectionDetails.EndPoint, cancellationToken)
			    {
				    LogExceptions = false
			    })
			    {
				    long pingId = Rnd.NextUInt();
				    long pingResult = 0;

				    ManualResetEventSlim ar = new ManualResetEventSlim(false);
				    QueryPacketHandler handler;

				    handler = new QueryPacketHandler(
					    conn, ar, response =>
					    {
						    waitingOnPing = false;
						    pingCallback?.Invoke(response);
					    }, (json) => { jsonResponse = json; });

				    conn.PacketHandler = handler;
				    handler.PingId = pingId;

				    bool connectionClosed = false;

				    using (cancellationToken.Register(
					           () =>
					           {
						           ar.Set();
					           }))
				    {
					    conn.OnConnectionClosed += (sender, args) =>
					    {
						    //if (!cancellationToken.IsCancellationRequested)
						    //	cancellationToken.Cancel();

						    connectionClosed = true;
						    ar.Set();
					    };

					    bool connected = conn.Initialize(cancellationToken);

					    if (connected)
					    {
						    var handshake = HandshakePacket.CreateObject();
						    handshake.NextState = ConnectionState.Status;
						    handshake.ServerAddress = connectionDetails.Hostname;
						    handshake.ServerPort = (ushort)connectionDetails.EndPoint.Port;
						    handshake.ProtocolVersion = JavaProtocol.ProtocolVersion;

						    conn.SendPacket(handshake);

						    conn.ConnectionState = ConnectionState.Status;

						    conn.SendPacket(RequestPacket.CreateObject());
					    }

					    if (connected && ar.Wait(1000, cancellationToken) && !connectionClosed && jsonResponse != null)
					    {

						    long timeElapsed = sw.ElapsedMilliseconds;
						    //  Log.Debug($"Server json: " + jsonResponse);
						    var query = ServerQuery.FromJson(jsonResponse);

						    if (query.Version.Protocol == JavaProtocol.ProtocolVersion)
						    {
							    query.Version.Compatibility = CompatibilityResult.Compatible;
						    }
						    else if (query.Version.Protocol < JavaProtocol.ProtocolVersion)
						    {
							    query.Version.Compatibility = CompatibilityResult.OutdatedServer;
						    }
						    else if (query.Version.Protocol > JavaProtocol.ProtocolVersion)
						    {
							    query.Version.Compatibility = CompatibilityResult.OutdatedClient;
						    }

						    var r = new ServerQueryStatus()
						    {
							    Delay = timeElapsed,
							    Success = true,
							    WaitingOnPing = pingCallback != null && waitingOnPing,
							    EndPoint = endPoint,
							    Address = connectionDetails.Hostname,
							    Port = (ushort)connectionDetails.EndPoint.Port,
							    Query = query
						    };

						    statusCallBack?.Invoke(new ServerQueryResponse(true, r));

						    if (waitingOnPing && pingCallback != null)
						    {
							    if (!ar.Wait(TimeSpan.FromMilliseconds(1000), cancellationToken)) { }
						    }
					    }
					    else
					    {
						    //conn = null;
						    statusCallBack?.Invoke(
							    new ServerQueryResponse(
								    false, "multiplayer.status.cannot_connect", new ServerQueryStatus()
								    {
									    EndPoint = endPoint,
									    Delay = sw.ElapsedMilliseconds,
									    Success = false,
									    /* Motd = motd.MOTD,
								             ProtocolVersion = motd.ProtocolVersion,
								             MaxNumberOfPlayers = motd.MaxPlayers,
								             Version = motd.ClientVersion,
								             NumberOfPlayers = motd.Players,*/
									    Address = connectionDetails.Hostname,
									    Port = (ushort)connectionDetails.EndPoint.Port,
									    WaitingOnPing = false
								    }));
					    }
				    }
			    }
		    }
		    catch (Exception ex)
		    {
			    if (sw.IsRunning)
				    sw.Stop();

			    // client?.Dispose();

			    string msg = ex.Message;

			    if (ex is SocketException || ex is OperationCanceledException)
			    {
				    msg = $"multiplayer.status.cannot_connect";
			    }
			    else
			    {
				    Log.Error(ex, $"Could not get server query result!");
			    }

			    statusCallBack?.Invoke(
				    new ServerQueryResponse(
					    false, msg,
					    new ServerQueryStatus()
					    {
						    Delay = sw.ElapsedMilliseconds,
						    Success = false,
						    EndPoint = endPoint,
						    Address = connectionDetails.Hostname,
						    Port = (ushort) connectionDetails.EndPoint.Port
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
    }
}
