using System;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;
using NLog;

namespace Alex.Networking.Bedrock
{
    public class AlexPacketFactory : ICustomPacketFactory
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(AlexPacketFactory));

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
            Packet packet = null;

            try
            {
                switch (messageId)
                {
                    case 111: //Fixes entity delta
                        packet = new EntityDelta();
                        break;
                    
                    //The following are only here so we can join.
                    case 49:
                        packet = new InventoryContent();
                        break;
                    case 31:
                        packet = new MobEquipment();
                        break;
                    case 122:
                        packet = new BiomeDefinitionList();
                        break;
                    case 119:
                        packet = new AvailableEntityIdentifiers();
                        break;
                    case 32:
                        packet = new MobArmorEquipment();
                        break;
                    case 50:
                        packet = new InventorySlot();
                        break;
                }
                
                packet?.Decode(buffer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Processing error....");
            }

            return packet;
        }
    }
}