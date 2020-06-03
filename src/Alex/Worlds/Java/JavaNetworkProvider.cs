using System;
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

namespace Alex.Worlds.Java
{
	public class JavaNetworkProvider : NetworkProvider
	{
		private JavaClient Client { get; }
		public JavaNetworkProvider(JavaClient client)
		{
			Client = client;
		}

		/// <inheritdoc />
		public override bool IsConnected => Client.IsConnected;

		public override ConnectionInfo GetConnectionInfo()
		{
			return new ConnectionInfo(DateTime.UtcNow, 0, 0, 0,0 ,0 ,0, 0, 0);
		}

		public override void EntityAction(int entityId, EntityAction action)
		{
			EntityActionPacket packet = new EntityActionPacket();
			packet.EntityId = entityId;
			packet.Action = action;
			packet.JumpBoost = 0;
			Client.SendPacket(packet);
		}

		/// <inheritdoc />
		public override void PlayerAnimate(PlayerAnimations animation)
		{
			//
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

	    public override void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition, Entity p)
	    {
		    Client.SendPacket(new PlayerBlockPlacementPacket()
	        {
                CursorPosition = cursorPosition,
                Location = position,
                Face = face,
                Hand = hand
	        });

        }

		public override void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
		{
			Client.SendPacket(new PlayerDiggingPacket()
			{
				Face = face,
				Location = position,
				Status = status
			});
		}

		public override void EntityInteraction(Entity player, Entity target, McpeInventoryTransaction.ItemUseOnEntityAction action)
		{
			
			switch (action)
			{
				case McpeInventoryTransaction.ItemUseOnEntityAction.Interact:
				{
					var packet = new InteractEntityPacket();
					packet.EntityId = (int) target.EntityId;
					packet.Type = 0;
					packet.Hand = 0;
					
					Client.SendPacket(packet);
				}
					break;
				case McpeInventoryTransaction.ItemUseOnEntityAction.Attack:
				{
					var packet = new InteractEntityPacket();
					packet.EntityId = (int) target.EntityId;
					packet.Type = 1;

					Client.SendPacket(packet);
				}
					break;
				case McpeInventoryTransaction.ItemUseOnEntityAction.ItemInteract:
					break;
			}
		}

		public override void WorldInteraction(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
		{
			throw new NotImplementedException();
		}

		public override void UseItem(Item item, int hand, ItemUseAction action)
		{
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
			
		}
	}
}