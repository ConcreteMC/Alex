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
            item = this.AlternativeReadItem();
            slot = ReadByte();
            selectedSlot = ReadByte();
            windowsId = ReadByte();
        }
    }
}