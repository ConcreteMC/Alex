using MiNET.Net;

namespace Alex.Net.Bedrock
{
    public class MobArmorEquipment : McpeMobArmorEquipment
    {
        /// <inheritdoc />
        protected override void DecodePacket()
        {
            this.Id = ReadByte();
            this.runtimeEntityId = this.ReadUnsignedVarLong();
            this.helmet = this.AlternativeReadItem();
            this.chestplate = this.AlternativeReadItem();
            this.leggings = this.AlternativeReadItem();
            this.boots = this.AlternativeReadItem();
        }
    }
}