using Alex.API.Entities;
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
	    void SendChatMessage(string message);
        void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition);
	    void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition);

	    void EntityInteraction(IEntity player, IEntity target,
		    McpeInventoryTransaction.ItemUseOnEntityAction action);

	    void WorldInteraction(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition);
	    void UseItem(int hand);
	    void HeldItemChanged(short slot);
	    void Close();
    }
}
