using NLog;

namespace Alex.Networking.Java.Packets
{
	public class DefaultPacketHandler : IPacketHandler
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(DefaultPacketHandler));

		private void UnhandledPacket(Packet packet)
		{
			Log.Warn($"Unhandled packet: 0x{packet.PacketId:X2} ({packet.GetType().Name})");
		}
		
		/// <inheritdoc />
		public void HandleHandshake(Packet packet)
		{
			UnhandledPacket(packet);
		}

		/// <inheritdoc />
		public void HandleStatus(Packet packet)
		{
			UnhandledPacket(packet);
		}

		/// <inheritdoc />
		public void HandleLogin(Packet packet)
		{
			UnhandledPacket(packet);
		}

		/// <inheritdoc />
		public void HandlePlay(Packet packet)
		{
			UnhandledPacket(packet);
		}
	}
}