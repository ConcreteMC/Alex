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
            this.helmet = this.AlternativeReadItem(false);
            this.chestplate = this.AlternativeReadItem(false);
            this.leggings = this.AlternativeReadItem(false);
            this.boots = this.AlternativeReadItem(false);
        }
    }
}