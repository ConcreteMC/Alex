#region Imports

using System;

#endregion

namespace Alex.Networking.Java.Events
{
    public class PacketReceivedEventArgs : EventArgs
    {
        public Packets.Packet Packet { get; }
        internal bool IsInvalid { get; set; } = false;
        internal PacketReceivedEventArgs(Packets.Packet netPacket)
        {
            Packet = netPacket;
        }

        public void Invalid()
        {
            IsInvalid = true;
        }
    }
}
