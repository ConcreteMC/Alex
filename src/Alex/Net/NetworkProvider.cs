using Alex.API;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Items;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;


namespace Alex.Net
{
	public abstract class NetworkProvider
	{
		public abstract bool IsConnected { get; }
		public abstract ConnectionInfo GetConnectionInfo();
		public abstract void EntityAction(int entityId, EntityAction action);

		public abstract void PlayerAnimate(PlayerAnimations animation);
		public abstract void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition, Entity player);
		public abstract void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition);

		public abstract void EntityInteraction(Entity player, Entity target,
			McpeInventoryTransaction.ItemUseOnEntityAction action);

		public abstract void WorldInteraction(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition);
		public abstract void UseItem(Item item, int hand, ItemUseAction action);
		public abstract void HeldItemChanged(Item item, short slot);
		public abstract void Close();
	}
}