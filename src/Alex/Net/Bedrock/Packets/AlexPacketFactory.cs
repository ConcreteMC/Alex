using System;
using System.Linq;
using Alex.Net.Bedrock.Raknet;
using MiNET.Net;
using MiNET.Net.RakNet;
using NLog;

namespace Alex.Net.Bedrock.Packets
{
    public class AlexPacketFactory : ICustomPacketFactory
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(AlexPacketFactory));
        
        public Packet Create(byte messageId, ReadOnlyMemory<byte> buffer, string ns)
        {
            if (ns == "raknet")
                return CreateRakNet(messageId, buffer);

            return CreatePe(messageId, buffer);
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
               // case 132:
              //      PrevBuffer = buffer.ToArray();
               //    break;
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
                 //   case 0x37:
                 ////       packet = new AdventureSettings();
                 //       break;
                  //  case 0x55:
                   //     packet = McpeTransfer.CreateObject();
                        //packet = new AdventureSettings();

                   //     break;
                //    case 0x0b:
               //         packet = new StartGame();
                //        break;
               //     case 0x34:
                //        packet = new CraftingData();
               //         break;
                    case 0x8a:
                        packet = McpeEmote.CreateObject();
                        break;
               //     case 0x91:
               //         packet = new CreativeContent();
                //        break;
                    case 0x9e:
                        packet = McpeAnimateEntity.CreateObject();
                        break;
                    //  case 0x07:
                  //      packet = new McpeResourcePackStack();
                  //      break;
                   // case 0x0c:
                   //     packet = new AddPlayer();
                   //     break;

                    //The following are only here so we can join.
                  //  case 0x1F:
                    //    packet = new MobEquipment();
                      //  break;
                  //  case 0x20:
                  //       packet = new MobArmorEquipment();

                  //       break;
                    //case 0x27:
                   //     packet = new SetEntityData();
                   //     break;
                  //  case 0x31:
                  //      packet = new InventoryContent();
                  //      break;
                 //   case 0x32:
                 //       packet = new InventorySlot();
                 //       break;
                    case 0x6f: //Fixes entity delta
                        packet = new EntityDelta();
                        break;
              //      case 0x77:
              //          packet = new AvailableEntityIdentifiers();
              //          break;
              //      case 0x7a:
              //          packet = new BiomeDefinitionList();
              //          break;
                }

                packet?.Decode(buffer);
            }
            catch (Exception ex)
            {
              //  if (messageId != 0x31)
                {
                      Log.Error(ex, $"Processing error.");
                }
            }

            return packet;
        }
    }
}