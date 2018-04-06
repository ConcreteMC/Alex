using System;
using System.Net.Sockets;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using NLog;

namespace Alex.Worlds.Java
{
	public class JavaClient : NetConnection
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaClient));

		private IJavaProvider WorldReceiver { get; }
		public JavaClient(JavaWorldProvider javaWorldProvider, Socket socket) : base(Direction.Server, socket)
		{
			MCPacketFactory.Load();

			WorldReceiver = javaWorldProvider;
		}

		protected override void HandlePacket(Packet packet)
		{
			if (packet == null) return;

			switch (ConnectionState)
			{
				case ConnectionState.Handshake:
					WorldReceiver.HandleHandshake(packet);
					break;
				case ConnectionState.Status:
					WorldReceiver.HandleStatus(packet);
					break;
				case ConnectionState.Login:
					WorldReceiver.HandleLogin(packet);
					break;
				case ConnectionState.Play:
					WorldReceiver.HandlePlay(packet);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
