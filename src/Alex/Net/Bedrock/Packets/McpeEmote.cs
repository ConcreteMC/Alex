using MiNET.Net;

namespace Alex.Net.Bedrock.Packets
{
	public class McpeEmote : Packet<McpeEmote>
	{
		public McpeEmote()
		{
			IsMcpe = true;
			Id = 0x8a;
		}

		public long RuntimeEntityId { get; set; }
		public string EmoteId { get; set; }
		public byte Flags { get; set; }
		
		/// <inheritdoc />
		protected override void DecodePacket()
		{
			base.DecodePacket();
			RuntimeEntityId = ReadUnsignedVarLong();
			EmoteId = ReadString();
			Flags = ReadByte();
		}
	}
}