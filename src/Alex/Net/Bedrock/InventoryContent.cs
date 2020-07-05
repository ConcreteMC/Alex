using MiNET.Net;

namespace Alex.Net.Bedrock
{
    public class InventoryContent : McpeInventoryContent
    {
        /// <inheritdoc />
        protected override void DecodePacket()
        {
            this.Id = ReadByte();
            this.inventoryId = this.ReadUnsignedVarInt();
            this.input = this.ReadItemStacksAlternate(true);
        }
    }
}