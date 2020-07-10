using System;
using System.Net.Sockets;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Play;
using MiNET.Utils;
using NLog;
using ConnectionState = Alex.Networking.Java.ConnectionState;
using DedicatedThreadPool = Alex.API.Utils.DedicatedThreadPool;
using Packet = Alex.Networking.Java.Packets.Packet;

namespace Alex.Worlds.Multiplayer.Java
{
	public class JavaClient : NetConnection
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaClient));

		private IJavaProvider WorldReceiver { get; }
		private JavaWorldProvider JavaWorld { get; }
		public JavaClient(JavaWorldProvider javaWorldProvider, Socket socket, DedicatedThreadPool networkPool) : base(PacketDirection.ClientBound, socket, null, networkPool)
		{
			MCPacketFactory.Load();
			JavaWorld = javaWorldProvider;
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
