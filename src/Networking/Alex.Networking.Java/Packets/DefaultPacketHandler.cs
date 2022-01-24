using System.Threading.Tasks;
using NLog;

namespace Alex.Networking.Java.Packets
{
	public class DefaultPacketHandler : IPacketHandler
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(DefaultPacketHandler));

		private Task UnhandledPacket(Packet packet)
		{
			Log.Warn($"Unhandled packet: 0x{packet.PacketId:X2} ({packet.GetType().Name})");

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task HandleHandshake(Packet packet)
		{
			return UnhandledPacket(packet);
		}

		/// <inheritdoc />
		public Task HandleStatus(Packet packet)
		{
			return UnhandledPacket(packet);
		}

		/// <inheritdoc />
		public Task HandleLogin(Packet packet)
		{
			return UnhandledPacket(packet);
		}

		/// <inheritdoc />
		public Task HandlePlay(Packet packet)
		{
			return UnhandledPacket(packet);
		}
	}
}