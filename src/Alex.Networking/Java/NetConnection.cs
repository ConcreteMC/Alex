#region Imports

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Utils;
using Alex.Networking.Java.Events;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Play;
using Alex.Networking.Java.Util;
using MonoGame.Framework.Utilities.Deflate;
using NLog;

#endregion

namespace Alex.Networking.Java
{
    public delegate void ConnectionConfirmed(NetConnection conn);
    public class NetConnection : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(NetConnection));
        
        private CancellationTokenSource CancellationToken { get; }
        private TcpClient Client { get; set; }
        public IPacketHandler PacketHandler { get; set; } = new DefaultPacketHandler();
        private IPEndPoint TargetEndpoint { get; }
		public NetConnection(IPEndPoint targetEndpoint, CancellationToken cancellationToken)
		{
			TargetEndpoint = targetEndpoint;
	      //  Socket = socket;

            CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			ConnectionState = ConnectionState.Handshake;
	        IsConnected = true;

			PacketWriteQueue = new BlockingCollection<EnqueuedPacket>();
        }
		
        public EventHandler<ConnectionClosedEventArgs> OnConnectionClosed;
        
		public ConnectionState ConnectionState { get; set; }
		public bool CompressionEnabled { get; set; }
		public int CompressionThreshold = 256;

		public bool IsConnected { get; private set; }

		private BlockingCollection<EnqueuedPacket> PacketWriteQueue { get; }
		public bool LogExceptions { get; set; } = true;

		public DateTime StartTime { get; private set; } = DateTime.UtcNow;
		public long     Latency   { get; set; }         = 0;

		public async Task<bool> Initialize(CancellationToken cancellationToken)
		{
			if (Client != null)
				return false;

			Client = new TcpClient();
			await Client.ConnectAsync(TargetEndpoint.Address, TargetEndpoint.Port, cancellationToken);

			if (!Client.Connected)
				return false;
			
			ThreadPool.QueueUserWorkItem(
				o =>
				{
					Thread.CurrentThread.Name = "MC:Java Network Thread";
					ProcessNetwork();
				});

			StartTime = DateTime.UtcNow;

			return true;
		}

		private bool _stopped = false;
		public void Stop()
		{
			if (_stopped)
				return;

			try
			{
				if (CancellationToken.IsCancellationRequested) return;
				CancellationToken.Cancel();

				if (SocketConnected(Client.Client))
				{
					//TODO
					Disconnected(true);
				}
				else
				{
					Disconnected(false);
				}
			}
			catch (SocketException) { }
			finally
			{
				_stopped = true;
			}
        }

        private object _disconnectSync = false;

        private void Disconnected(bool notified)
        {
	        try
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

		       // Client.Client.Shutdown(SocketShutdown.Both);
		        Client?.Close();

		        OnConnectionClosed?.Invoke(this, new ConnectionClosedEventArgs(this, notified));

		        IsConnected = false;
	        }
	        catch (ObjectDisposedException)
	        {
		        //Ok
	        }
        }

	    public void InitEncryption(byte[] sharedKey)
	    {
			_readerStream.InitEncryption(sharedKey);
			_sendStream.InitEncryption(sharedKey);
	    }

	    //public static RecyclableMemoryStreamManager StreamManager { get; }= new RecyclableMemoryStreamManager();
	    private MinecraftStream _readerStream;

	    private int _lastReceivedPacketId;
	    private void ProcessNetwork()
	    {
		    Stopwatch time           = Stopwatch.StartNew();
		   
		    try
		    {
			    using (NetworkStream ns = Client.GetStream())
			    {
				    using (MinecraftStream writeStream = new MinecraftStream(ns, CancellationToken.Token))
				    using (MinecraftStream readStream = new MinecraftStream(ns, CancellationToken.Token))
				    {
					    SpinWait sw = new SpinWait();
					    _readerStream = readStream;
					    _sendStream = writeStream;
					    
					    while (!CancellationToken.IsCancellationRequested)
					    {
						    if (CancellationToken.IsCancellationRequested)
							    break;
						    
						    if (!Read(readStream) && !Write(writeStream))
							    sw.SpinOnce();
						    //Write(mc);
					    }
				    }
			    }
		    }
		    catch (Exception ex)
		    {
			  //  if (ex is OperationCanceledException) return;
			 //   if (ex is EndOfStreamException) return;
			 //   if (ex is IOException) return;

			    if (LogExceptions)
				    Log.Warn(
					    $"Failed to process network (Last packet: 0x{_lastReceivedPacketId:X2} State: {ConnectionState}): " +
					    ex);
		    }
		    finally
		    {
			    Disconnected(false);
		    }
	    }

	    private bool Read(MinecraftStream stream)
	    {
		    if (!stream.DataAvailable)
			    return false;

		    if (TryReadPacket(stream, out var lastPacketId))
		    {
			    _lastReceivedPacketId = lastPacketId;
			    return true;
		    }


		    return false;
	    }

	    private bool Write(MinecraftStream stream)
	    {
		    if (PacketWriteQueue.TryTake(out var packet, 0))
		    {
			    try
			    {
				    var data = EncodePacket(packet);

				    Interlocked.Increment(ref PacketsOut);
				    Interlocked.Add(ref PacketSizeOut, data.Length);

				    stream.WriteVarInt(data.Length);
				    stream.Write(data);
			    }
			    finally
			    {
				    packet.Packet.PutPool();
			    }

			    return true;
		    }

		    return false;
	    }
	    
	    public long PacketsIn;
	    public long PacketsOut;
	    public long PacketSizeIn;
	    public long PacketSizeOut;
	    private bool TryReadPacket(MinecraftStream stream, out int lastPacketId)
	    {
		    Packets.Packet packet = null;
		    int            packetId;
		    byte[] packetData;

		    if (!CompressionEnabled)
		    {
			    int length = stream.ReadVarInt();

			    int packetIdLength;
			    packetId = stream.ReadVarInt(out packetIdLength);
			    lastPacketId = packetId;

			    if (length - packetIdLength > 0)
			    {
				    packetData = stream.Read(length - packetIdLength);
			    }
			    else
			    {
				    packetData = new byte[0];
			    }
		    }
		    else
		    {
			    int packetLength = stream.ReadVarInt();

			    int br;
			    int dataLength = stream.ReadVarInt(out br);

			    int readMore;

			    if (dataLength == 0)
			    {
				    packetId = stream.ReadVarInt(out readMore);
				    lastPacketId = packetId;
				    packetData = stream.Read(packetLength - (br + readMore));
			    }
			    else
			    {
				    var data = stream.ReadToSpan(packetLength - br);

				    using (MinecraftStream a = new MinecraftStream(CancellationToken.Token))
				    {
					    using (ZlibStream outZStream = new ZlibStream(
						    a, CompressionMode.Decompress, CompressionLevel.Default, true))
					    {
						    outZStream.Write(data);
						  //  outZStream.Write(data, 0, data.Length);
					    }

					    a.Seek(0, SeekOrigin.Begin);

					    int l;
					    packetId = a.ReadVarInt(out l);
					    lastPacketId = packetId;
					    packetData = a.Read(dataLength - l);
				    }
			    }
		    }

		    packet = MCPacketFactory.GetPacket(ConnectionState, packetId);

		    try
		    {
			    Interlocked.Increment(ref PacketsIn);
			    Interlocked.Add(ref PacketSizeIn, packetData.Length);

			    if (packet == null)
			    {
				    if (UnhandledPacketsFilter[ConnectionState].TryAdd(packetId, 1))
				    {
					    Log.Debug(
						    $"Unhandled packet in {ConnectionState}! 0x{packetId.ToString("x2")} = {(ConnectionState == ConnectionState.Play ? MCPacketFactory.GetPlayPacketName(packetId) : "Unknown")}");
				    }
				    else
				    {
					    UnhandledPacketsFilter[ConnectionState][packetId] =
						    UnhandledPacketsFilter[ConnectionState][packetId] + 1;
				    }

				    return false;
			    }

			    //    Log.Info($"Received: {packet}");

			    ProcessPacket(packet, packetData);
			    return true;
		    }
		    finally
		    {
			    packet?.PutPool();
		    }

		    return false;
	    }
	    
	    private void ProcessPacket(Packet packet, byte[] data)
		{
			packet.Stopwatch.Start();
			
			using (var memoryStream = new MemoryStream(data))
			{
				using (MinecraftStream minecraftStream = new MinecraftStream(memoryStream, CancellationToken.Token))
				{
					packet.Decode(minecraftStream);
				}
			}
			
			HandlePacket(packet);
			
			packet.Stopwatch.Stop();
			if (packet.Stopwatch.ElapsedMilliseconds > 250)
			{
				Log.Warn($"Packet handling took too long: {packet.GetType()} | {packet.Stopwatch.ElapsedMilliseconds}ms Processed bytes: {data.Length} (Queue size: 0)");
			}
		}


	    private Dictionary<ConnectionState, ConcurrentDictionary<int, int>> UnhandledPacketsFilter =
		    new Dictionary<ConnectionState, ConcurrentDictionary<int, int>>()
		    {
			    {ConnectionState.Handshake, new ConcurrentDictionary<int, int>() },
			    {ConnectionState.Status, new ConcurrentDictionary<int, int>() },
			    {ConnectionState.Login, new ConcurrentDictionary<int, int>() },
			    {ConnectionState.Play, new ConcurrentDictionary<int, int>() },
			};


		protected virtual void HandlePacket(Packets.Packet packet)
	    {
		    switch (ConnectionState)
		    {
			    case ConnectionState.Handshake:
				    PacketHandler.HandleHandshake(packet);
				    break;

			    case ConnectionState.Status:
				    PacketHandler.HandleStatus(packet);
				    break;

			    case ConnectionState.Login:
				    PacketHandler.HandleLogin(packet);
				    break;

			    case ConnectionState.Play:
				    PacketHandler.HandlePlay(packet);
				    break;
		    }
	    }

		public void SendPacket(Packet packet)
	    {
		    if (PacketWriteQueue.IsAddingCompleted)
		    {
			    return;
		    }
		    
			if (packet.PacketId == -1) throw new Exception();
			
			PacketWriteQueue.Add(new EnqueuedPacket(packet, CompressionEnabled));
	    }

		private MinecraftStream _sendStream;

	    private byte[] EncodePacket(EnqueuedPacket enqueued)
	    {
		    var packet = enqueued.Packet;
		    byte[] encodedPacket;
		    using (MemoryStream ms = new MemoryStream())
		    {
			    using (MinecraftStream mc = new MinecraftStream(ms, CancellationToken.Token))
			    {
				    mc.WriteVarInt(packet.PacketId);
				    packet.Encode(mc);

				    encodedPacket = ms.ToArray();

				    mc.Position = 0;
				    mc.SetLength(0);

				    if (enqueued.CompressionEnabled)
				    {
					    if (encodedPacket.Length >= CompressionThreshold)
					    {
						    //byte[] compressed;
						    //CompressData(encodedPacket, out compressed);

						    mc.WriteVarInt(encodedPacket.Length);
						    using (ZlibStream outZStream = new ZlibStream(mc, CompressionMode.Compress, CompressionLevel.Default, true))
						    {
							    outZStream.Write(encodedPacket, 0, encodedPacket.Length);
						    }
						   // mc.Write(compressed);
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

		    return encodedPacket;
	    }

	    private bool SocketConnected(Socket s)
        {
	        try
	        {
		        bool part1 = s.Poll(1000, SelectMode.SelectRead);
		        bool part2 = (s.Available == 0);
		        if (part1 && part2)
			        return false;
		        else
			        return true;
	        }
	        catch
	        {
		        return false;
	        }
        }

	    public void Dispose()
	    {
			Stop();

		   // NetworkProcessing?.Wait();
			//NetworkProcessing?.Dispose();
		  //  NetworkProcessing = null;
		    ClearOutQueue(PacketWriteQueue);

			//NetworkWriting?.Wait();
			//NetworkWriting?.Dispose();
		//	NetworkWriting = null;
			//PacketWriteQueue?.Dispose();

		  //  ClearOutQueue(HandlePacketQueue);

			//PacketHandling?.Wait();
			//PacketHandling?.Dispose();
		//	PacketHandling = null;

			//HandlePacketQueue?.Dispose();

		    CancellationToken?.Dispose();

		    _readerStream?.Dispose();
		    _sendStream?.Dispose();
		    Client?.Dispose();

			foreach (var state in UnhandledPacketsFilter.ToArray())
		    {
			    foreach (var p in state.Value)
			    {
					Log.Warn($"({state.Key}) unhandled: 0x{p.Key:X2} ({(state.Key == ConnectionState.Play ? MCPacketFactory.GetPlayPacketName(p.Key) : "Unknown")}) * {p.Value}");
			    }
		    }

			UnhandledPacketsFilter.Clear();
		}

	    private void ClearOutQueue<TType>(BlockingCollection<TType> collection)
	    {
			collection.CompleteAdding();
		    while (collection.TryTake(out var _, 0)) {};
	    }

	    private struct EnqueuedPacket
	    {
		    public Packet Packet;
		    public bool CompressionEnabled;

		    public EnqueuedPacket(Packet packet, bool compression)
		    {
			    Packet = packet;
			   // Encryption = encryption;
			    CompressionEnabled = compression;
		    }
	    }
    }
}
