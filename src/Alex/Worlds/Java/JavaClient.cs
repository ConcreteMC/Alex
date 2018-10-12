using System;
using System.Net.Sockets;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Play;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Java
{
	public class JavaClient : NetConnection, INetworkProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(JavaClient));

		private IJavaProvider WorldReceiver { get; }
		public JavaClient(JavaWorldProvider javaWorldProvider, Socket socket) : base(Direction.ClientBound, socket, null, null)
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


		public void EntityAction(int entityId, EntityAction action)
		{
			EntityActionPacket packet = new EntityActionPacket();
			packet.EntityId = entityId;
			packet.Action = action;
			packet.JumpBoost = 0;
			SendPacket(packet);
		}

		public void SendChatMessage(string message)
		{
			SendPacket(new ChatMessagePacket()
			{
				Position = ChatMessagePacket.Chat,
				Message = message,
				ServerBound = true
			});
		}
	}
}
