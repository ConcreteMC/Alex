using MiNET.Net;
using MiNET.Worlds;

namespace Alex.Net.Bedrock.Packets
{
	public class McpeUpdateGm : McpeUpdatePlayerGameType
	{
		public GameMode GameMode;
		public long PlayerEntityUniqueId;

		public McpeUpdateGm() { }

		/// <inheritdoc />
		protected override void DecodePacket()
		{
			base.DecodePacket();
			GameMode = (GameMode)ReadVarInt();
			PlayerEntityUniqueId = ReadVarLong();
		}
	}
}