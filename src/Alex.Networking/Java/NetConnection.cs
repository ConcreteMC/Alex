#region Imports

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Alex.Networking.Java.Events;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Util;
using Ionic.Zlib;
using NLog;

#endregion

namespace Alex.Networking.Java
{
    public delegate void ConnectionConfirmed(NetConnection conn);
    public class NetConnection
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(NetConnection));
        
        private CancellationTokenSource CancellationToken { get; }
        protected ConnectionConfirmed ConnectionConfirmed { get; }
        private Direction Direction { get; }
        private Socket Socket { get; }
        public NetConnection(Direction direction, Socket socket, ConnectionConfirmed confirmdAction = null)
        {
            Direction = direction;
            Socket = socket;
            RemoteEndPoint = Socket.RemoteEndPoint;

            ConnectionConfirmed = confirmdAction;

            CancellationToken = new CancellationTokenSource();

			ConnectionState = ConnectionState.Handshake;
	        IsConnected = true;

			PacketWriteQueue = new BlockingCollection<byte[]>();
        }

        public EventHandler<PacketReceivedEventArgs> OnPacketReceived;
        public EventHandler<ConnectionClosedEventArgs> OnConnectionClosed;

        public EndPoint RemoteEndPoint { get; private set; }
		public ConnectionState ConnectionState { get; set; }
		public bool CompressionEnabled { get; set; }
		public int CompressionThreshold = 256;

	    public bool EncryptionInitiated { get; private set; } = false;
		protected byte[] SharedSecret { get; private set; }

		public bool IsConnected { get; private set; }

		private BlockingCollection<byte[]> PacketWriteQueue { get; }

		private Task NetworkProcessing { get; set; }
		private Task NetworkWriting { get; set; }
        public void Initialize()
        {
	        Socket.Blocking = true;

            NetworkProcessing = new Task(ProcessNetwork, CancellationToken.Token);
            NetworkProcessing.Start();

			NetworkWriting = new Task(SendQueue, CancellationToken.Token);
			NetworkWriting.Start();
        }

        public void Stop()
        {
            if (CancellationToken.IsCancellationRequested) return;
            CancellationToken.Cancel();

            if (SocketConnected(Socket))
            {
                //TODO
                Disconnected(true);
            }
            else
            {
                Disconnected(false);
            }
        }

        private object _disconnectSync = false;

        private void Disconnected(bool notified)
        {
            lock (_disconnectSync)
            {
                if ((bool) _disconnectSync) return;
                _disconnectSync = true;
            }

            if (!CancellationToken.IsCancellationRequested)
            {
                CancellationToken.Cancel();
            }

            Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();

            OnConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(this, notified));

	        IsConnected = false;
        }

	    public void InitEncryption(byte[] sharedKey)
	    {
		    SharedSecret = sharedKey;
			_readerStream.InitEncryption(SharedSecret, false);
			_sendStream.InitEncryption(SharedSecret, true);
		    EncryptionInitiated = true;
	    }

	    private MinecraftStream _readerStream;
		private void ProcessNetwork()
		{
			int lastPacketId = 0;
			try
            {
                using (NetworkStream ns = new NetworkStream(Socket))
                {
                    using (MinecraftStream ms = new MinecraftStream(ns))
                    {
	                    _readerStream = ms;

                        while (!CancellationToken.IsCancellationRequested)
                        {
	                        Packets.Packet packet = null;
	                        int packetId;
							byte[] packetData;

							if (!CompressionEnabled)
	                        {
		                        int length = ms.ReadVarInt();

		                        int packetIdLength;
		                        packetId = ms.ReadVarInt(out packetIdLength);
		                        lastPacketId = packetId;
		                        if (length - packetIdLength > 0)
		                        {
			                        /*packetData = new byte[length - packetIdLength];
			                        int read = 0;
			                        while (read < packetData.Length)
			                        {
				                        read += ms.Read(packetData, read, packetData.Length - read);

				                        if (CancellationToken.IsCancellationRequested) throw new OperationCanceledException();
			                        }*/
			                        packetData = ms.Read(length - packetIdLength);
		                        }
		                        else
		                        {
			                        packetData = new byte[0];
		                        }
	                        }
	                        else
							{
								int packetLength = ms.ReadVarInt();

								int br;
								int dataLength = ms.ReadVarInt(out br);

								int readMore;
								if (dataLength == 0)
								{
									packetId = ms.ReadVarInt(out readMore);
									lastPacketId = packetId;
									packetData = ms.Read(packetLength - (br + readMore));
								}
								else
								{
									byte[] data = ms.Read(packetLength - br);
									byte[] decompressed;
									DecompressData(data, out decompressed);

									using (MemoryStream b = new MemoryStream(decompressed))
									{
										using (MinecraftStream a = new MinecraftStream(b))
										{
											int l;
											packetId = a.ReadVarInt(out l);
											lastPacketId = packetId;
											packetData = a.Read(dataLength - l);
										}
									}
								}
							}

							packet = MCPacketFactory.GetPacket(Direction, ConnectionState, packetId);
							if (packet == null)
							{
								Log.Warn($"Unhandled packet in {ConnectionState}! 0x{packetId.ToString("x2")}");
								continue;
							}
	                       
							packet.Decode(new MinecraftStream(new MemoryStream(packetData)));
							HandlePacket(packet);
						}
                    }
                }
            }
            catch(Exception ex)
            {
	            Log.Warn($"OH NO (0x{lastPacketId:X2}): " + ex);
                if (ex is OperationCanceledException) return;
                if (ex is EndOfStreamException) return;
                if (ex is IOException) return;

                Log.Error("An unhandled exception occured while processing network!", ex);
            }
            finally
            {
                Disconnected(false);
            }
        }

	    protected virtual void HandlePacket(Packets.Packet packet)
	    {
			PacketReceivedEventArgs args = new PacketReceivedEventArgs(packet);
		    OnPacketReceived?.BeginInvoke(this, args, PacketReceivedCallback, args);
	    }

        private void PacketReceivedCallback(IAsyncResult ar)
        {
            OnPacketReceived.EndInvoke(ar);
            PacketReceivedEventArgs args = (PacketReceivedEventArgs)ar.AsyncState;
            if (args.IsInvalid)
            {
                Log.Warn("Packet reported as invalid!");
            }
        }

        private void SendDataInternal(byte[] buffer)
        {
	        if (CancellationToken.IsCancellationRequested) return;
            SendData sendData = new SendData(buffer);
            Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallback, sendData);
        }

        private void SendCallback(IAsyncResult ar)
        {
	        try
	        {
		        SocketError result;
		        int sent = Socket.EndSend(ar, out result);

		        SendData data = (SendData) ar.AsyncState;

		        if (result == SocketError.Success)
		        {
			        if (sent != data.Buffer.Length)
			        {
				        Log.Warn("Sent {0} out of {1} bytes!", sent, data.Buffer.Length);
			        }
		        }
		        else
		        {
			        Log.Warn("Failed to send data! (Reason: {0})", result);
		        }
			}
			catch { }
        }

	    public void SendPacket(Packet packet)
	    {
			if (packet.PacketId == -1) throw new Exception();

		    byte[] encodedPacket;
			using (MemoryStream ms = new MemoryStream())
		    {
			    using (MinecraftStream mc = new MinecraftStream(ms))
			    {
					mc.WriteVarInt(packet.PacketId);
					packet.Encode(mc);

					encodedPacket = ms.ToArray();

				    mc.Position = 0;
					mc.SetLength(0);

					if (CompressionEnabled)
					{
						if (encodedPacket.Length >= CompressionThreshold)
						{
							byte[] compressed;
							CompressData(encodedPacket, out compressed);

							mc.WriteVarInt(encodedPacket.Length);
							mc.Write(compressed);
						}
						else //Uncompressed
						{
							mc.WriteVarInt(0);
							mc.Write(encodedPacket);
						}

						encodedPacket = ms.ToArray();
					}
				}
		    }

			Log.Info($"Sending packet ({CompressionEnabled}:{EncryptionInitiated}): {packet} 0x{packet.PacketId:X2}");

			PacketWriteQueue.Add(encodedPacket);
	    }

	    private MinecraftStream _sendStream;
	    private void SendQueue()
	    {
		    using (NetworkStream ms = new NetworkStream(Socket))
		    {
			    using (MinecraftStream mc = new MinecraftStream(ms))
			    {
				    _sendStream = mc;
				    while (!CancellationToken.IsCancellationRequested)
				    {
					    try
					    {
						    byte[] data = PacketWriteQueue.Take(CancellationToken.Token);
							mc.WriteVarInt(data.Length);
							mc.Write(data);
						}
					    catch (OperationCanceledException)
					    {
						    break;
					    }
				    }
			    }
		    }
	    }

		public static void CompressData(byte[] inData, out byte[] outData)
		{
			using (MemoryStream outMemoryStream = new MemoryStream())
			{
				using (ZlibStream outZStream = new ZlibStream(outMemoryStream, CompressionMode.Compress, CompressionLevel.Default, true))
				{
					outZStream.Write(inData, 0, inData.Length);
				}
				outData = outMemoryStream.ToArray();
			}
		}

		public static void DecompressData(byte[] inData, out byte[] outData)
		{
			using (MemoryStream outMemoryStream = new MemoryStream())
			{
				using (ZlibStream outZStream = new ZlibStream(outMemoryStream, CompressionMode.Decompress, CompressionLevel.Default, true))
				{
					outZStream.Write(inData, 0, inData.Length);
				}
				outData = outMemoryStream.ToArray();
			}
		}

		private bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
    }

    internal struct ReceivedData
    {
        public byte[] OldBuffer;
        public byte[] Buffer;
        public DateTime Time;

        public ReceivedData(byte[] buffer, byte[] oldBuffer)
        {
            Buffer = buffer;
            OldBuffer = oldBuffer;
            Time = DateTime.UtcNow;
        }
    }

    internal struct SendData
    {
        public byte[] Buffer;
        public DateTime Time;

        public SendData(byte[] buffer)
        {
            Buffer = buffer;
            Time = DateTime.UtcNow;
        }
    }
}
