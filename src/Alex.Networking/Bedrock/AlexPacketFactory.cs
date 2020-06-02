using System;
using System.Linq;
using Alex.Networking.Bedrock.Net.Raknet;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;
using NLog;

namespace Alex.Networking.Bedrock
{
    public class AlexPacketFactory : ICustomPacketFactory
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(AlexPacketFactory));
        
        private byte[] PrevBuffer { get; set; }
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
                case 0x03:
                    packet = new CustomConnectedPong();
                    break;
                
                //Hack to spawn on the Hive
                case 132:
                    PrevBuffer = buffer.ToArray();
                    break;
                case 16:
                    packet = new RequestAccepted();
                    break;
            }

            try
            {
                packet?.Decode(buffer);
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                Log.Warn(ex, $"RakNet processing: {ex.ToString()}");
                if (PrevBuffer != null)
                {
                    Memory<byte> newBuffer = new Memory<byte>(PrevBuffer.Concat(buffer.ToArray()).ToArray());

                    packet?.Decode(newBuffer);
                }
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
                    case 7:
                        packet = new McpeResourcePackStack();
                        break;
                    case 39:
                        packet = new McpeSetEntityData();
                        break;
                }

                packet?.Decode(buffer);
            }
            catch (Exception ex)
            {
                if (messageId != 39)
                {
                //    Log.Error(ex, $"Processing error: {ex.ToString()}");
                }
            }

            return packet;
        }
    }
}