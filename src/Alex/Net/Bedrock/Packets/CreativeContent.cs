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
			base.DecodePacket();
			//Id = IsMcpe ? (byte) ReadVarInt() : ReadByte();

			//this.input = ReadItemStacks();
		}
	}
}