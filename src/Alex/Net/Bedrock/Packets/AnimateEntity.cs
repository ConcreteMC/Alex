using MiNET.Net;

namespace Alex.Net.Bedrock.Packets
{
	public class McpeAnimateEntity : Packet<McpeAnimateEntity>
	{
		public McpeAnimateEntity()
		{
			Id = 0x9e;
			this.IsMcpe = true;
		}

		public string Animation { get; set; }
		public string NextState { get; set; }
		public string StopCondition { get; set; }
		public string Controller { get; set; }
		public float BlendOutTime { get; set; }
		public long[] EntityRuntimeIds { get; set; }

		/// <inheritdoc />
		protected override void DecodePacket()
		{
			base.DecodePacket();
			Animation = ReadString();
			NextState = ReadString();
			StopCondition = ReadString();
			Controller = ReadString();
			BlendOutTime = ReadFloat();

			var count = ReadUnsignedVarInt();
			EntityRuntimeIds = new long[count];

			for (int i = 0; i < count; i++)
			{
				EntityRuntimeIds[i] = ReadUnsignedVarLong();
			}
		}
	}
}