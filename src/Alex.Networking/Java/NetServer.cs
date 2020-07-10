#region Imports

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Alex.Networking.Java.Events;
using NLog;

#endregion

namespace Alex.Networking.Java
{
    public class NetServer
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(NetServer));
        
		public NetConnectionFactory NetConnectionFactory { get; set; }

		private CancellationTokenSource CancelationToken { get; set; }
        private ConcurrentDictionary<EndPoint, NetConnection> Connections { get; set; }

		internal NetConfiguration Configuration { get; }
        private Socket ListenerSocket { get; set; }
        public NetServer(NetConfiguration configuration)
        {
            Configuration = configuration;
            SetDefaults();
            Configure();
        }

        public EventHandler<ConnectionAcceptedEventArgs> OnConnectionAccepted;

        private void SetDefaults()
        {
            CancelationToken = new CancellationTokenSource();

            Connections = new ConcurrentDictionary<EndPoint, NetConnection>();
			NetConnectionFactory = new NetConnectionFactory();

		}

        private void Configure()
        {
            if (Configuration.Protocol == ProtocolType.Tcp)
            {
                ListenerSocket = new Socket(AddressFamily.InterNetwork,
                                      SocketType.Stream,
                                      System.Net.Sockets.ProtocolType.Tcp);
            }
            else
            {
                throw new NotSupportedException("This protocol is currently not supported!");
            }
        }

        public void Start()
        {
            if (Configuration.Protocol == ProtocolType.Tcp)
            {
                ListenerSocket.Bind(new IPEndPoint(Configuration.Host, Configuration.Port));
                ListenerSocket.Listen(10);

                ListenerSocket.BeginAccept(ConnectionCallback, null);
            }
        }

        public void Stop()
        {
            CancelationToken.Cancel();

            foreach (var i in Connections)
            {
                i.Value.Stop();
            }
        }

        private void ConnectionCallback(IAsyncResult ar)
        {
            Socket socket = null;
            try
            {
                socket = ListenerSocket.EndAccept(ar);
            }
            catch
            {
                Log.Warn("Failed to accept connection!");
            }

            ListenerSocket.BeginAccept(ConnectionCallback, null);

            if (socket == null) return;
			
			NetConnection conn = NetConnectionFactory.CreateConnection(PacketDirection.ServerBound, socket, ConfirmdAction);

			if (Connections.TryAdd(socket.RemoteEndPoint, conn))
			{
				conn.OnConnectionClosed += (sender, args) =>
				{
					NetConnection nc;
					if (Connections.TryRemove(args.Connection.RemoteEndPoint, out nc))
					{
						Log.Info("Client disconnected!");
					}
				};
				conn.Initialize();
			}
			else
			{
				Log.Warn("Could not create new active connection!");
			}
		}

        private void ConfirmdAction(NetConnection conn)
        {
            OnConnectionAccepted?.Invoke(this, new ConnectionAcceptedEventArgs(conn));
        }
    }
}
