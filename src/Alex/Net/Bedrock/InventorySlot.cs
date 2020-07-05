using MiNET.Net;

namespace Alex.Net.Bedrock
{
    public class InventorySlot : McpeInventorySlot
    {
        /// <inheritdoc />
        protected override void DecodePacket()
        {
            this.Id = this.ReadByte();
            
            inventoryId = ReadUnsignedVarInt();
            slot = ReadUnsignedVarInt();
            Write(item);
            item = this.AlternativeReadItem(true);
        }
    }
}