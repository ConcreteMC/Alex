using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
    public class MessageHandler : ICustomMessageHandler
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        private readonly McpeClientMessageDispatcher _messageDispatcher;

        public                     Action               ConnectionAction   { get; set; }
        public                     Action<string, bool> DisconnectedAction { get; set; }
        
        private protected readonly RakSession           _session;

        public CryptoContext CryptoContext { get; set; }
        
        public MessageHandler(RakSession session, IMcpeClientMessageHandler handler) : base()
        {
	        _session = session;
            _messageDispatcher = new McpeClientMessageDispatcher(handler);
        }

        public void Connected()
        {
            ConnectionAction?.Invoke();
        }

        public void Disconnect(string reason, bool sendDisconnect = true)
        {
            DisconnectedAction?.Invoke(reason, sendDisconnect);
        }

        public List<Packet> PrepareSend(List<Packet> packetsToSend)
		{
			var sendList = new List<Packet>();
			var sendInBatch = new List<Packet>();

			foreach (Packet packet in packetsToSend)
			{
				// We must send forced clear messages in single message batch because
				// we can't mix them with un-encrypted messages for obvious reasons.
				// If need be, we could put these in a batch of it's own, but too rare 
				// to bother.
				if (packet.ForceClear)
				{
					var wrapper = McpeWrapper.CreateObject();
					wrapper.ReliabilityHeader.Reliability = Reliability.ReliableOrdered;
					wrapper.ForceClear = true;
					wrapper.payload = Compression.CompressPacketsForWrapper(new List<Packet> {packet});
					wrapper.Encode(); // prepare
					packet.PutPool();
					sendList.Add(wrapper);
					continue;
				}

				if (packet is McpeWrapper)
				{
					packet.ReliabilityHeader.Reliability = Reliability.ReliableOrdered;
					sendList.Add(packet);
					continue;
				}

				if (!packet.IsMcpe)
				{
					packet.ReliabilityHeader.Reliability = packet.ReliabilityHeader.Reliability != Reliability.Undefined ? packet.ReliabilityHeader.Reliability : Reliability.Reliable;
					sendList.Add(packet);
					continue;
				}

				packet.ReliabilityHeader.Reliability = Reliability.ReliableOrdered;

				sendInBatch.Add(OnSendCustomPacket(packet));
			}

			if (sendInBatch.Count > 0)
			{
				var batch = McpeWrapper.CreateObject();
				batch.ReliabilityHeader.Reliability = Reliability.ReliableOrdered;
				batch.payload = Compression.CompressPacketsForWrapper(sendInBatch);
				batch.Encode(); // prepare
				sendList.Add(batch);
			}

			return sendList;
		}

		public Packet HandleOrderedSend(Packet packet)
		{
			if (!packet.ForceClear && CryptoContext != null && CryptoContext.UseEncryption && packet is McpeWrapper wrapper)
			{
				var encryptedWrapper = McpeWrapper.CreateObject();
				encryptedWrapper.ReliabilityHeader.Reliability = Reliability.ReliableOrdered;
				encryptedWrapper.payload = CryptoUtils.Encrypt(wrapper.payload, CryptoContext);
				encryptedWrapper.Encode();

				return encryptedWrapper;
			}

			return packet;
		}

		public void HandlePacket(Packet message)
		{
			if (message is McpeWrapper wrapper)
			{
				var messages = new LinkedList<Packet>();

				// Get bytes to process
				ReadOnlyMemory<byte> payload = wrapper.payload;

				// Decrypt bytes

				if (CryptoContext != null && CryptoContext.UseEncryption)
				{
					// This call copies the entire buffer, but what can we do? It is kind of compensated by not
					// creating a new buffer when parsing the packet (only a mem-slice)
					payload = CryptoUtils.Decrypt(payload, CryptoContext);
				}
				
				var stream = new MemoryStreamReader(payload);
				using (var deflateStream = new DeflateStream(stream, CompressionMode.Decompress, false))
				{
					using var s = new MemoryStream();
					deflateStream.CopyTo(s);
					s.Position = 0;

					int count = 0;
					// Get actual packet out of bytes
					while (s.Position < s.Length)
					{
						count++;

						uint len = VarInt.ReadUInt32(s);
						long pos = s.Position;
						ReadOnlyMemory<byte> internalBuffer = s.GetBuffer().AsMemory((int) s.Position, (int) len);
						int id = VarInt.ReadInt32(s);
						try
						{
							var packet = PacketFactory.Create((byte) id, internalBuffer, "mcpe")
							             ?? new UnknownPacket((byte) id, internalBuffer);

							//Hack for some servers that screw up the order.
							if (packet is McpePlayerList)
							{
								messages.AddFirst(packet);
							}
							else
							{
								messages.AddLast(packet);
							}
						}
						catch (Exception e)
						{
							if (Log.IsDebugEnabled) Log.Warn($"Error parsing bedrock message #{count} id={id}\n{Packet.HexDump(internalBuffer)}", e);
							//throw;
							return; // Exit, but don't crash.
						}

						s.Position = pos + len;
					}

					if (s.Length > s.Position) throw new Exception("Have more data");
				}

				//var msgs = messages.ToArray();
				//messages.Clear();
				

				foreach (Packet msg in messages)
				{
					msg.ReliabilityHeader = new ReliabilityHeader()
					{
						Reliability = wrapper.ReliabilityHeader.Reliability,
						ReliableMessageNumber = wrapper.ReliabilityHeader.ReliableMessageNumber,
						OrderingChannel = wrapper.ReliabilityHeader.OrderingChannel,
						OrderingIndex = wrapper.ReliabilityHeader.OrderingIndex,
					};

					try
					{
						HandleCustomPacket(msg);
					}
					catch (Exception e)
					{
						Log.Warn(e,$"Bedrock message handler error");
					}
				}

				wrapper.PutPool();
			}
			else if (message is UnknownPacket unknownPacket)
			{
				if (Log.IsDebugEnabled) Log.Warn($"Received unknown packet 0x{unknownPacket.Id:X2}\n{Packet.HexDump(unknownPacket.Message)}");

				unknownPacket.PutPool();
			}
			else
			{
				Log.Error($"Unhandled packet: {message.GetType().Name} 0x{message.Id:X2} for user: {_session.Username}, IP {_session.EndPoint.Address}");
				if (Log.IsDebugEnabled) Log.Warn($"Unknown packet 0x{message.Id:X2}\n{Packet.HexDump(message.Bytes)}");
			}
		}
        
        public Packet OnSendCustomPacket(Packet message)
        {
	     //   Log.Info($"Sent: {message}");
            return message;
        }

        public void HandleCustomPacket(Packet message)
        {
            Stopwatch sw = Stopwatch.StartNew();

            try
            {
	         //   Log.Info($"Got: {message}");
                _messageDispatcher.HandlePacket(message);
            }
            catch (Exception ex)
            {
               // if (message.Id == 39)
               //     return;
                Log.Warn(ex, $"Packet handling error: {message} - {ex.ToString()}");
            }
            finally
            {
                sw.Stop();

                if (sw.ElapsedMilliseconds > 250)
                {
                    Log.Warn($"Packet handling took longer than expected! Time elapsed: {sw.ElapsedMilliseconds}ms (Packet={message})");
                }
            }
        }
    }
}