using Alex.Items;
using MiNET.Net;
using MiNET.UI;
using MiNET.Utils;

namespace Alex.Net.Bedrock.Packets
{
	public class CreativeContent : McpeCreativeContent
	{
		/// <inheritdoc />
		protected override void DecodePacket()
		{
			//base.DecodePacket();
			Id = IsMcpe ? (byte) ReadVarInt() : ReadByte();
			
			var metadata = new ItemStacks();
			
			var count    = ReadUnsignedVarInt();
			for (int i = 0; i < count; i++)
			{
				var id   = ReadVarInt();
				var slot = this.ReadItem2();

				slot.UniqueId = id;
				metadata.Add(slot);
			}

			this.input = metadata;
		}
	}
}