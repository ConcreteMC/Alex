using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Alex.API.Utils;
using Alex.Net.Bedrock;
using Microsoft.IO;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;
using NLog;
using Org.BouncyCastle.Utilities.Zlib;
using CompressionLevel = MonoGame.Framework.Utilities.Deflate.CompressionLevel;
using CompressionMode = MonoGame.Framework.Utilities.Deflate.CompressionMode;
using CryptoContext = MiNET.Utils.CryptoContext;

namespace Alex.Worlds.Multiplayer.Bedrock
{
    public class MessageHandler : ICustomMessageHandler
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        private readonly McpeClientMessageDispatcher _messageDispatcher;

        public                     Action               ConnectionAction   { get; set; }
        public                     Action<string, bool> DisconnectedAction { get; set; }
        
        private readonly RaknetSession           _session;

        public CryptoContext CryptoContext { get; set; }

        private DateTime _lastPacketReceived;
        public  TimeSpan TimeSinceLastPacket => DateTime.UtcNow - _lastPacketReceived;
        
        public MessageHandler(RaknetSession session, IMcpeClientMessageHandler handler) : base()
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
					wrapper.payload = Compress(new List<Packet>(){packet});
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

				sendInBatch.Add(packet);
			}

			if (sendInBatch.Count > 0)
			{
				var batch = McpeWrapper.CreateObject();
				batch.ReliabilityHeader.Reliability = Reliability.ReliableOrdered;

				if (CryptoContext != null && CryptoContext.UseEncryption)
				{
					batch.payload = CryptoUtils.Encrypt(Compress(sendInBatch), CryptoContext);
				}
				else
				{
					batch.payload = Compress(sendInBatch);
				}

				batch.Encode(); // prepare
				sendList.Add(batch);
			}

			return sendList;
		}

        private byte[] Compress(ICollection<Packet> packets)
        {
	        long length = 0;
	        foreach (Packet packet in packets) length += packet.Encode().Length;

	        var compressionLevel = _session.CompressionThreshold > -1 && length >= _session.CompressionThreshold ?
		        System.IO.Compression.CompressionLevel.Fastest : System.IO.Compression.CompressionLevel.NoCompression;

	        using (MemoryStream stream = MiNetServer.MemoryStreamManager.GetStream())
	        {
		        int checksum;

		        using (var compressStream = new DeflateStream(stream, compressionLevel, true))
		        {
			        foreach (Packet packet in packets)
			        {
				        byte[] bs = packet.Encode();

				        if (bs != null && bs.Length > 0)
				        {
					        BatchUtils.WriteLength(compressStream, bs.Length);
					        compressStream.Write(bs, 0, bs.Length);
				        }

				        packet.PutPool();
			        }

			        compressStream.Flush();
		        }

		        byte[] bytes = stream.ToArray();

		        return bytes;
	        }
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
			if (_session.Evicted)
				return;
			
			if (message is McpeWrapper wrapper)
			{
				//var messages = new List<Packet>();

				// Get bytes to process
				var payload = wrapper.payload.ToArray();

				// Decrypt bytes


				if (CryptoContext != null && CryptoContext.UseEncryption)
				{
					//FirstEncryptedPacketWaitHandle.WaitOne();
					
					payload = CryptoUtils.Decrypt(payload, CryptoContext);
				}

				using (var stream = new MemoryStream(payload))
				{
					using (var deflateStream = new DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress, true))
					{
						payload = deflateStream.ReadAllBytes();
					}
				}

				using (MemoryStream ms = new MemoryStream(payload))
				{
					int count = 0;

					// Get actual packet out of bytes
					while (ms.Position < ms.Length - 1)
					{
						uint   len  = VarInt.ReadUInt32(ms);
						long   pos  = ms.Position;
						byte[] data = new byte[len];
						if (ms.Read(data, 0, data.Length) != len)
							Log.Warn(
								$"Did not read enough data.");
								
						ms.Position = pos;
						int id = VarInt.ReadInt32(ms);

						Packet packet = null;

						try
						{
							packet = PacketFactory.Create((byte) id, data, "mcpe")
							         ?? new UnknownPacket((byte) id, data);

							packet.ReliabilityHeader = wrapper.ReliabilityHeader;

							//messages.Add(packet);
							count++;
						}
						catch (Exception e)
						{
							Log.Warn(
								e, $"Error parsing bedrock message #{count} id={id} (Buffer size={data.Length} Packet size={len})");

									
							//throw;
							//return; // Exit, but don't crash.
						}


						if (packet != null)
						{
							try
							{
								HandleGamePacket(packet);
							}
							catch (Exception e)
							{
								Log.Warn(e, $"Error handling game packet #{count} id={id}");
							}
							
							packet.PutPool();
						}

						ms.Position = pos + len;
					}
				}

				//var msgs = messages.ToArray();
				//messages.Clear();
				//wrapper.PutPool();
			}
			else if (message is UnknownPacket unknownPacket)
			{
				Log.Warn($"Received unknown packet 0x{unknownPacket.Id:X2}\n{Packet.HexDump(unknownPacket.Message)}");

			//	unknownPacket.PutPool();
			}
			else
			{
				Log.Error($"Unhandled packet: {message.GetType().Name} 0x{message.Id:X2}, IP {_session.EndPoint.Address}");
				if (Log.IsDebugEnabled) Log.Warn($"Unknown packet 0x{message.Id:X2}\n{Packet.HexDump(message.Bytes)}");
			}
		}

		private void HandleGamePacket(Packet message)
		{
			if (_session.Evicted)
				return;
			
			RaknetSession.TraceReceive(message);
			
			_lastPacketReceived = DateTime.UtcNow;
			Stopwatch sw = Stopwatch.StartNew();

			try
			{
				//     Log.Info($"Got packet: {message}");
				if (!_messageDispatcher.HandlePacket(message))
				{
					if (message is UnknownPacket unknownPacket)
					{
						Log.Warn($"Received unknown packet 0x{unknownPacket.Id:X2}\n{Packet.HexDump(unknownPacket.Message)}");
					}
				}
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
					Log.Warn(
						$"Packet handling took longer than expected! Time elapsed: {sw.ElapsedMilliseconds}ms (Packet={message})");
				}
			}
		}
    }
}