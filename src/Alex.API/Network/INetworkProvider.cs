using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using MiNET;

namespace Alex.API.Network
{
    public interface INetworkProvider
    {
	    bool IsConnected { get; }
		void EntityAction(int entityId, EntityAction action);
	    void SendChatMessage(string message);
        void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition);
    }
}
