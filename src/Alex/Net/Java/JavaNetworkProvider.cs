using System;
using System.Threading;
using Alex.Common;
using Alex.Common.Items;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Items;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets.Play;
using Microsoft.Xna.Framework;
using MiNET.Utils.IO;
using BlockFace = Alex.Common.Blocks.BlockFace;
using ConnectionState = Alex.Networking.Java.ConnectionState;
using Player = Alex.Entities.Player;

namespace Alex.Net.Java
{
	public class JavaNetworkProvider : NetworkProvider
	{
		private NetConnection Client            { get; }
	//	private HighPrecisionTimer      NetworkReportTimer { get; }
		public JavaNetworkProvider(NetConnection client)
		{
			Client = client;
			
			/*NetworkReportTimer =  new HighPrecisionTimer(
						1000, state =>
						{
							long   packetSizeOut = Interlocked.Exchange(ref Client.PacketSizeOut, 0L);
							long   packetSizeIn = Interlocked.Exchange(ref Client.PacketSizeIn, 0L);

							long   packetCountOut = Interlocked.Exchange(ref Client.PacketsOut, 0L);
							long   packetCountIn = Interlocked.Exchange(ref Client.PacketsIn, 0L);

							_connectionInfo = new ConnectionInfo(
								Client.StartTime, Client.Latency, -1, -1, -1, -1, -1,
								packetSizeIn, packetSizeOut, packetCountIn, packetCountOut);
						});*/
		}

		/// <inheritdoc />
		public override bool IsConnected => Client.IsConnected;
		protected override ConnectionInfo GetConnectionInfo()
		{
			long   packetSizeOut = Interlocked.Exchange(ref Client.PacketSizeOut, 0L);
			long   packetSizeIn = Interlocked.Exchange(ref Client.PacketSizeIn, 0L);

			long   packetCountOut = Interlocked.Exchange(ref Client.PacketsOut, 0L);
			long   packetCountIn = Interlocked.Exchange(ref Client.PacketsIn, 0L);

			return new ConnectionInfo(
				Client.StartTime, Client.Latency, -1, -1, -1, -1, -1,
				packetSizeIn, packetSizeOut, packetCountIn, packetCountOut);
		}

		/// <inheritdoc />
		public override void PlayerOnGroundChanged(Player player, bool onGround)
		{
			if (Client.ConnectionState != ConnectionState.Play)
				return;

			PlayerMovementPacket packet = PlayerMovementPacket.CreateObject();
			packet.OnGround = onGround;
			
			Client.SendPacket(packet);
		}

		/// <inheritdoc />
		public override void EntityFell(long entityId, float distance, bool inVoid)
		{
			
		}

		public override void EntityAction(int entityId, EntityAction action)
		{
			if (action >= Common.Utils.EntityAction.Jump)
				return;

			EntityActionPacket packet = EntityActionPacket.CreateObject();
			
			packet.EntityId = entityId;
			packet.Action = action;
			packet.JumpBoost = 0;
			Client.SendPacket(packet);
		}

		/// <inheritdoc />
		public override void PlayerAnimate(PlayerAnimations animation)
		{
			var packet = AnimationPacket.CreateObject();
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
			var packet = ChatMessagePacket.CreateObject();
			packet.Position = ChatMessagePacket.Chat;
			packet.Message = message;
			packet.ServerBound = true;
			
			Client.SendPacket(packet);
		}

	    public override void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, int slot, Vector3 cursorPosition, Entity p)
	    {
		    if (hand < 0) hand = 0;
		    if (hand > 1) hand = 1;
		    
		    var packet = PlayerBlockPlacementPacket.CreateObject();
		    packet.CursorPosition = cursorPosition;
		    packet.Location = position;
		    packet.Face = face;
		    packet.Hand = hand;
		    packet.InsideBlock = p.HeadInBlock;
		    
		    Client.SendPacket(packet);
	    }

		public override void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
		{
			var packet = PlayerDiggingPacket.CreateObject();
			packet.Face = face;
			packet.Location = position;
			packet.Status = status;
			Client.SendPacket(packet);
		}

		public override void EntityInteraction(Entity player, Entity target, ItemUseOnEntityAction action, int hand, int slot, Vector3 cursorPosition)
		{
			if (hand < 0) hand = 0;
			if (hand > 1) hand = 1;
			
			switch (action)
			{
				case ItemUseOnEntityAction.Interact:
				{
					var packet = InteractEntityPacket.CreateObject();
					packet.EntityId = (int) target.EntityId;
					packet.Type = 0;
					packet.Hand = hand;
					packet.Sneaking = player.IsSneaking;
					
					Client.SendPacket(packet);
				}
					break;
				case ItemUseOnEntityAction.Attack:
				{
					var packet = InteractEntityPacket.CreateObject();
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

		public override void WorldInteraction(Entity entity, BlockCoordinates position, BlockFace face, int hand, int slot, Vector3 cursorPosition)
		{
			if (hand < 0) hand = 0;
			if (hand > 1) hand = 1;
			
			var packet = PlayerBlockPlacementPacket.CreateObject();
			packet.Location = position;
			packet.Face = face;
			packet.Hand = hand;
			packet.CursorPosition = cursorPosition;
			packet.InsideBlock = entity.HeadInBlock;
			
			Client.SendPacket(packet);
		}

		public override void UseItem(Item item, int hand, ItemUseAction action, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
		{
			if (hand > 1)
				hand = 1;

			if (hand < 0)
				hand = 0;
			
			var packet = UseItemPacket.CreateObject();
			packet.Hand = hand;
			Client.SendPacket(packet);
		}

		public override void HeldItemChanged(Item item, short slot)
		{
			var packet = HeldItemChangePacket.CreateObject();
			packet.Slot = slot;
			
			Client.SendPacket(packet);
		}

		/// <inheritdoc />
		public override void DropItem(BlockCoordinates position, BlockFace face, Item item, bool dropFullStack)
		{
			var packet = PlayerDiggingPacket.CreateObject();
			packet.Face = face;
			packet.Location = position;
			packet.Status = DiggingStatus.DropItem;
			Client.SendPacket(packet);
		}

		public override void Close()
		{
			//NetworkReportTimer?.Change(Timeout.Infinite, Timeout.Infinite);
			
		}

		/// <inheritdoc />
		public override void SendChatMessage(ChatObject message)
		{
			SendChatMessage(message.RawMessage);
		}

		/// <inheritdoc />
		public override void RequestRenderDistance(int oldValue, int newValue)
		{
			SendSettings(_skinParts, _mainHand == 1, newValue);
		}

		private byte _skinParts;
		private int _mainHand = 0;
		public void SendSettings(byte skinFlags, bool isLeftHanded, int renderDistance)
		{
			ClientSettingsPacket settings = ClientSettingsPacket.CreateObject();
			settings.ChatColors = true;
			settings.ChatMode = 0;
			settings.ViewDistance = (byte)  renderDistance;
			_skinParts = settings.SkinParts = skinFlags; // 255;
			_mainHand = settings.MainHand = isLeftHanded ? 0 : 1;
			settings.Locale = Alex.Instance.GuiRenderer.Language.Code; //Options.MiscelaneousOptions.Language.Value;
			
			Client.SendPacket(settings);
		}
	}
}