#region Imports

using System;
using System.Net.Sockets;
using Alex.Networking.Java.Events;

#endregion

namespace Alex.Networking.Java
{
	public class NetConnectionFactory
	{
		public EventHandler<NetConnectionCreatedEventArgs> OnConnectionCreated;

		internal NetConnection CreateConnection(PacketDirection packetDirection, Socket socket, ConnectionConfirmed confirmedAction = null)
		{
			NetConnection connection = Create(packetDirection, socket, confirmedAction);

			if (connection == null) return null;

			OnConnectionCreated?.Invoke(null, new NetConnectionCreatedEventArgs(connection));
			return connection;
		}

		protected virtual NetConnection Create(PacketDirection packetDirection, Socket socket, ConnectionConfirmed confirmedAction = null)
		{
			NetConnection conn = new NetConnection(packetDirection, socket, confirmedAction);
			return conn;
		}
	}
}
