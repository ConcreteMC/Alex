using System.Threading.Tasks;

namespace Alex.Networking.Java.Packets
{
	public interface IPacketHandler
	{
		Task HandleHandshake(Packet packet);

		Task HandleStatus(Packet packet);

		Task HandleLogin(Packet packet);

		Task HandlePlay(Packet packet);
	}
}