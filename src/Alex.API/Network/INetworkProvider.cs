using Alex.API.Utils;

namespace Alex.API.Network
{
    public interface INetworkProvider
    {
	    void EntityAction(int entityId, EntityAction action);
	    void SendChatMessage(string message);

    }
}
