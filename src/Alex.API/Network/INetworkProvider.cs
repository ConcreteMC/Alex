using Alex.API.Entities;
using Alex.API.Items;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;

namespace Alex.API.Network
{
    public interface INetworkProvider
    {
	    bool IsConnected { get; }
		void EntityAction(int entityId, EntityAction action);

		void PlayerAnimate(PlayerAnimations animation);
	    void SendChatMessage(string message);
        void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition, IEntity player);
	    void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition);

	    void EntityInteraction(IEntity player, IEntity target,
		    McpeInventoryTransaction.ItemUseOnEntityAction action);

	    void WorldInteraction(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition);
	    void UseItem(IItem item, int hand);
	    void HeldItemChanged(IItem item, short slot);
	    void Close();
    }
}
