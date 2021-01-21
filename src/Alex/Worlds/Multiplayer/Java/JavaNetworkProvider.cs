using System;
using System.Threading;
using Alex.API;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java.Packets.Play;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;
using ConnectionState = Alex.Networking.Java.ConnectionState;
using Player = Alex.Entities.Player;

namespace Alex.Worlds.Multiplayer.Java
{
	public class JavaNetworkProvider : NetworkProvider
	{
		private JavaClient Client            { get; }
		private Timer      NetworkReportTimer { get; }
		public JavaNetworkProvider(JavaClient client)
		{
			Client = client;
			
			NetworkReportTimer =  new Timer(
						state =>
						{
							long   packetSizeOut = Interlocked.Exchange(ref Client.PacketSizeOut, 0L);
							long   packetSizeIn = Interlocked.Exchange(ref Client.PacketSizeIn, 0L);

							long   packetCountOut = Interlocked.Exchange(ref Client.PacketsOut, 0L);
							long   packetCountIn = Interlocked.Exchange(ref Client.PacketsIn, 0L);

							_connectionInfo = new ConnectionInfo(
								Client.StartTime, Client.Latency, 0, 0, 0, 0, 0,
								packetSizeIn, packetSizeOut, packetCountIn, packetCountOut);
						}, null, 1000L, 1000L);
		}

		/// <inheritdoc />
		public override bool IsConnected => Client.IsConnected;

		private ConnectionInfo _connectionInfo = new ConnectionInfo(DateTime.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
		public override ConnectionInfo GetConnectionInfo()
		{
			return _connectionInfo;
		}

		/// <inheritdoc />
		public override void PlayerOnGroundChanged(Player player, bool onGround)
		{
			if (Client.ConnectionState != ConnectionState.Play)
				return;
			
			Client.SendPacket(new PlayerMovementPacket()
			{
				OnGround = onGround
			});
		}

		/// <inheritdoc />
		public override void EntityFell(long entityId, float distance, bool inVoid)
		{
			
		}

		public override void EntityAction(int entityId, EntityAction action)
		{
			if (action == API.Utils.EntityAction.Jump)
				return;
			
			EntityActionPacket packet = new EntityActionPacket();
			packet.EntityId = entityId;
			packet.Action = action;
			packet.JumpBoost = 0;
			Client.SendPacket(packet);
		}

		/// <inheritdoc />
		public override void PlayerAnimate(PlayerAnimations animation)
		{
			AnimationPacket packet = new AnimationPacket();
			switch (animation)
			{
				case PlayerAnimations.SwingLeftArm:
					packet.Hand = 1;
					break;

				case PlayerAnimations.SwingRightArm:
					packet.Hand = 0;
					break;
			}
			
			Client.SendPacket(packet);
		}

		public void SendChatMessage(string message)
		{
			Client.SendPacket(new ChatMessagePacket()
			{
				Position = ChatMessagePacket.Chat,
				Message = message,
				ServerBound = true
			});
		}

	    public override void BlockPlaced(BlockCoordinates position, API.Blocks.BlockFace face, int hand, int slot, Vector3 cursorPosition, Entity p)
	    {
		    Client.SendPacket(new PlayerBlockPlacementPacket()
	        {
                CursorPosition = cursorPosition,
                Location = position,
                Face = face,
                Hand = hand,
                InsideBlock = p.HeadInBlock
	        });

        }

		public override void PlayerDigging(DiggingStatus status, BlockCoordinates position, API.Blocks.BlockFace face, Vector3 cursorPosition)
		{
			Client.SendPacket(new PlayerDiggingPacket()
			{
				Face = face,
				Location = position,
				Status = status
			});
		}

		public override void EntityInteraction(Entity player, Entity target, ItemUseOnEntityAction action, int hand, int slot)
		{
			
			switch (action)
			{
				case ItemUseOnEntityAction.Interact:
				{
					var packet = new InteractEntityPacket();
					packet.EntityId = (int) target.EntityId;
					packet.Type = 0;
					packet.Hand = hand;
					packet.Sneaking = player.IsSneaking;
					
					Client.SendPacket(packet);
				}
					break;
				case ItemUseOnEntityAction.Attack:
				{
					var packet = new InteractEntityPacket();
					packet.EntityId = (int) target.EntityId;
					packet.Type = 1;
					packet.Hand = hand;
					packet.Sneaking = player.IsSneaking;
					
					Client.SendPacket(packet);
				}
					break;
				case ItemUseOnEntityAction.ItemInteract:
					break;
			}
		}

		public override void WorldInteraction(Entity entity, BlockCoordinates position, API.Blocks.BlockFace face, int hand, int slot, Vector3 cursorPosition)
		{
			Client.SendPacket(new PlayerBlockPlacementPacket()
			{
				Location = position,
				Face = face,
				Hand = hand,
				CursorPosition = cursorPosition,
				InsideBlock = entity.HeadInBlock
			});
		}

		public override void UseItem(Item item, int hand, ItemUseAction action, BlockCoordinates position, API.Blocks.BlockFace face, Vector3 cursorPosition)
		{
			//if (!(action == ))
			Client.SendPacket(new UseItemPacket()
			{
				Hand = hand
			});
		}

		public override void HeldItemChanged(Item item, short slot)
		{
			Client.SendPacket(new HeldItemChangePacket()
			{
				Slot = slot
			});
		}

		public override void Close()
		{
			NetworkReportTimer?.Change(Timeout.Infinite, Timeout.Infinite);
			NetworkReportTimer?.Dispose();
		}

		/// <inheritdoc />
		public override void SendChatMessage(ChatObject message)
		{
			SendChatMessage(message.RawMessage);
		}
	}
}