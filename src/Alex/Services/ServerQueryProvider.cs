using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Handshake;
using Alex.Networking.Java.Packets.Status;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alex.Services
{
    public class ServerQueryProvider : IServerQueryProvider
    {
        public ServerQueryProvider()
        {
            MCPacketFactory.Load();
        }

        public async Task<ServerQueryResponse> QueryServerAsync(string hostname, ushort port)
        {
            return await QueryJavaServerAsync(hostname, port);
        }

        private async Task<ServerQueryResponse> QueryJavaServerAsync(string hostname, ushort port)
        {
            var sw = Stopwatch.StartNew();
	        TcpClient client = null;
	        NetConnection conn = null;
			IPEndPoint endPoint = null;
	        try
	        {
		        client = new TcpClient();

		        await client.ConnectAsync(hostname, port);
		        endPoint = client.Client.RemoteEndPoint as IPEndPoint;

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
					        var json = JsonConvert.DeserializeObject<ServerListPingJson>(jsonResponse);

							return new ServerQueryResponse(true, new ServerQueryStatus()
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
					        });
				        }
			        }
		        }
	        }
	        catch (Exception ex)
	        {
		        if (sw.IsRunning)
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

                            Version = serverData[2],
                            Motd = serverData[3],
                            NumberOfPlayers = int.Parse(serverData[4]),
                            MaxNumberOfPlayers = int.Parse(serverData[5]),

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
}
