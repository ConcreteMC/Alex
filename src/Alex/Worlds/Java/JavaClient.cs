using System;
using System.Net.Sockets;
using Alex.API.Entities;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Play;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using ConnectionState = Alex.Networking.Java.ConnectionState;
using Packet = Alex.Networking.Java.Packets.Packet;

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

	    public void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
	    {
	        SendPacket(new PlayerBlockPlacementPacket()
	        {
                CursorPosition = cursorPosition,
                Location = position,
                Face = face,
                Hand = hand
	        });

        }

		public void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
		{
			SendPacket(new PlayerDiggingPacket()
			{
				Face = face,
				Location = position,
				Status = status
			});
		}

		public void EntityInteraction(IEntity player, IEntity target, McpeInventoryTransaction.ItemUseOnEntityAction action)
		{
			throw new NotImplementedException();
		}

		public void WorldInteraction(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
		{
			throw new NotImplementedException();
		}

		public void UseItem(int hand)
		{
			SendPacket(new UseItemPacket()
			{
				Hand = hand
			});
		}

		public void HeldItemChanged(short slot)
		{
			SendPacket(new HeldItemChangePacket()
			{
				Slot = slot
			});
		}

		public void Close()
		{
			base.Dispose();
		}
	}
}
