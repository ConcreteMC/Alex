using System;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;

namespace Alex.Networking.Bedrock
{
    public class AlexPacketFactory : ICustomPacketFactory
    {
        public Packet Create(byte messageId, ReadOnlyMemory<byte> buffer, string ns)
        {
            if (ns == "raknet")
                return CreateRakNet(messageId, buffer);

            return CreatePe(messageId, buffer);
            
            return null;
        }

        private Packet CreateRakNet(byte messageId, ReadOnlyMemory<byte> buffer)
        {
            Packet packet = null;
            switch (messageId)
            {
                
            }
            
            return packet;
        }

        private Packet CreatePe(byte messageId, ReadOnlyMemory<byte> buffer)
        {
            return null;
        }
    }
}