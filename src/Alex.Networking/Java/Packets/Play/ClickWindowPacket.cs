using Alex.API.Data;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class ClickWindowPacket : Packet<ClickWindowPacket>
    {
        public byte WindowId { get; set; }
        public short Slot { get; set; }
        public byte Button { get; set; }
        public short Action { get; set; }
        public TransactionMode Mode { get; set; }
        public SlotData ClickedItem { get; set; }
        
        public ClickWindowPacket()
        {
            PacketId = 0x09;
        }
        
        public override void Decode(MinecraftStream stream)
        {
            throw new System.NotImplementedException();
        }

        public override void Encode(MinecraftStream stream)
        {
            stream.WriteByte(WindowId);
            stream.WriteShort(Slot);
            stream.WriteByte(Button);
            stream.WriteShort(Action);
            stream.WriteVarInt((int) Mode);
            stream.WriteSlot(ClickedItem);
        }

        public enum TransactionMode
        {
            Click = 0,
            ShiftClick = 1,
            NumberKey = 2,
            MiddleClick = 3,
            Drop = 4,
            MouseDrag = 5,
            DoubleClick = 6
        }
    }
}