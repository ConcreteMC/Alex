using fNbt;
using MiNET.Items;
using MiNET.Net;

namespace Alex.Net.Bedrock
{
    public class MobEquipment : McpeMobEquipment
    {
        /// <inheritdoc />
        protected override void DecodePacket()
        {
            this.Id = ReadByte();
            runtimeEntityId = ReadUnsignedVarLong();
            item = this.AlternativeReadItem(false);
            slot = ReadByte();
            selectedSlot = ReadByte();
            windowsId = ReadByte();
        }

        /// <inheritdoc />
        protected override void EncodePacket()
        {
            WriteVarInt(this.Id);
            this.WriteUnsignedVarLong(this.runtimeEntityId);
            this.AlternativeWriteItem(this.item);
            this.Write(this.slot);
            this.Write(this.selectedSlot);
            this.Write(this.windowsId);
        }
    }
}