namespace Alex.Networking.Java.Packets
{
	public interface IPacketHandler
	{
		void HandleHandshake(Packet packet);
		void HandleStatus(Packet packet);
		void HandleLogin(Packet packet);
		void HandlePlay(Packet packet);
	}
}