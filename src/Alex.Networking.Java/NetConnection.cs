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
using Alex.Networking.Java.Events;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Login;
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

        public ConnectionState ConnectionState
        {
            get => _connectionState;
            set
            {
                _connectionState = value;

                if (value == ConnectionState.Play)
                {

                }
            }
        }

        public bool CompressionEnabled { get; set; }
        public int CompressionThreshold = 256;

        public bool IsConnected { get; private set; }

        private BlockingCollection<EnqueuedPacket> PacketWriteQueue { get; }
        public bool LogExceptions { get; set; } = true;

        public DateTime StartTime { get; private set; } = DateTime.UtcNow;
        public long Latency { get; set; } = 0;

        private Thread _readThread;
        private Thread _writeThread;
        public bool Initialize(CancellationToken cancellationToken)
        {
            try
            {
                if (Client != null)
                    return false;

                Client = new TcpClient();

                //Client.ReceiveBufferSize = 1024;
                //Client.SendBufferSize = 1024;
                //Client.NoDelay = true;
                //Client.Client.DontFragment = false;

                Client.Connect(TargetEndpoint.Address, TargetEndpoint.Port);

                if (!Client.Connected)
                    return false;

                NetworkStream ns = Client.GetStream();
                _readerStream = new MinecraftStream(ns, CancellationToken.Token);
                _writerStream = new MinecraftStream(ns, CancellationToken.Token);

                _readThread = new Thread(ProcessNetworkRead) { Name = "MC:Java Network Read" };


                _writeThread = new Thread(ProcessNetworkWrite) { Name = "MC:Java Network Write" };

                _readThread.Start();
                _writeThread.Start();
            }
            catch (SocketException exception)
            {
                if (exception.SocketErrorCode == SocketError.ConnectionRefused)
                    return false;
            }

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
                    if ((bool)_disconnectSync) return;
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

        private byte[] _sharedKey = null;
        public void InitEncryption(byte[] sharedKey)
        {
            _sharedKey = sharedKey;
        }

        //public static RecyclableMemoryStreamManager StreamManager { get; }= new RecyclableMemoryStreamManager();
        private MinecraftStream _readerStream;

        private int _lastReceivedPacketId;
        private int _lastSentPacketId;
        private int[] _lastSentPacketIds = new int[10];
        private object _readLock = new object();

        private void ProcessNetworkRead()
        {
            Stopwatch time = Stopwatch.StartNew();


            try
            {
                SpinWait sw = new SpinWait();

                while (!CancellationToken.IsCancellationRequested)
                {
                    if (CancellationToken.IsCancellationRequested)
                        break;

                    var stream = _readerStream;

                    if (stream == null)
                        break;

                    if (stream.DataAvailable)
                    {
                        time.Restart();
                        if (TryReadPacket(stream, out var lastPacketId))
                        {
                            var elapsed = time.Elapsed;

                            if (elapsed >= TimeSpan.FromMilliseconds(500))
                            {
                                //  Log.Warn($"Packet Read took: {elapsed.TotalMilliseconds:F2}ms");
                            }
                        }
                        //_lastReceivedPacketId = lastPacketId;
                    }
                    else
                    {
                        sw.SpinOnce();
                    }
                    //Write(mc);
                }

            }
            catch (Exception ex)
            {
                //  if (ex is OperationCanceledException) return;
                //   if (ex is EndOfStreamException) return;
                //   if (ex is IOException) return;

                if (LogExceptions)
                {
                    Log.Warn(
                        ex,
                        $"Failed read from network (Last packet read=0x{_lastReceivedPacketId:X2}, last packet written=0x{_lastSentPacketId:X2}, State: {ConnectionState})");

                    for (int i = 0; i < _lastSentPacketIds.Length; i++)
                    {
                        Log.Debug($"Sent: 0x{_lastSentPacketIds[i]:X2}");
                    }
                }
            }
            finally
            {
                Disconnected(false);
            }
        }

        private void ProcessNetworkWrite()
        {
            try
            {

                SpinWait sw = new SpinWait();

                while (!CancellationToken.IsCancellationRequested)
                {
                    if (CancellationToken.IsCancellationRequested)
                        break;

                    var stream = _writerStream;

                    if (stream == null)
                        break;

                    var queue = PacketWriteQueue;

                    if (!queue.TryTake(out var packet, 10, CancellationToken.Token))
                        continue;


                    //Send(packet, stream);
                    try
                    {
                        //    Log.Info($"Sent packet: {packet.Packet} (0x{packet.Packet.PacketId:X2})");
                        var data = EncodePacket(packet);

                        Interlocked.Increment(ref PacketsOut);
                        Interlocked.Add(ref PacketSizeOut, data.Length);

                        stream.WriteVarInt(data.Length);
                        stream.Write(data);

                        if (packet.Packet is EncryptionResponsePacket)
                        {
                            // lock (_readLock)
                            {
                                var aes = MinecraftStream.GenerateAES(_sharedKey);
                                _readerStream.InitEncryption(aes);
                                _writerStream.InitEncryption(aes);
                            }

                            Log.Info($"Encryption enabled.");
                        }
                    }
                    finally
                    {
                        int[] copy = new int[_lastSentPacketIds.Length];

                        for (int i = 1; i < _lastSentPacketIds.Length; i++)
                        {
                            copy[i - 1] = _lastSentPacketIds[i];
                        }

                        copy[^1] = packet.Packet.PacketId;
                        _lastSentPacketIds = copy;

                        _lastSentPacketId = packet.Packet.PacketId;
                        packet.Packet.PutPool();
                    }
                    //Write(mc);
                }

            }
            catch (Exception ex)
            {
                //  if (ex is OperationCanceledException) return;
                //   if (ex is EndOfStreamException) return;
                // if (ex is IOException) return;

                if (LogExceptions)
                {
                    Log.Warn(
                        ex,
                        $"Failed write to network (Last packet read=0x{_lastReceivedPacketId:X2}, last packet written=0x{_lastSentPacketId:X2}, State: {ConnectionState})");

                    for (int i = 0; i < _lastSentPacketIds.Length; i++)
                    {
                        Log.Debug($"Sent: 0x{_lastSentPacketIds[i]:X2}");
                    }
                }
            }
            finally
            {
                Disconnected(false);
            }

            //   Log.Warn($"Write thread exited!");
        }

        private object _sendLock = new object();

        public long PacketsIn;
        public long PacketsOut;
        public long PacketSizeIn;
        public long PacketSizeOut;

        private bool TryReadPacket(MinecraftStream stream, out int lastPacketId)
        {
            lastPacketId = 0;
            Packets.Packet packet = null;
            int packetId;
            byte[] packetData;

            lock (_readLock)
            {
                int length = stream.ReadVarInt(out int read);

                if (read == 0 || length == -1)
                    return false;

                if (!CompressionEnabled)
                {
                    int packetIdLength;
                    packetId = stream.ReadVarInt(out packetIdLength);
                    _lastReceivedPacketId = lastPacketId = packetId;

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
                    int br;
                    int dataLength = stream.ReadVarInt(out br);

                    int readMore;

                    if (dataLength == 0)
                    {
                        packetId = stream.ReadVarInt(out readMore);
                        _lastReceivedPacketId = lastPacketId = packetId;
                        packetData = stream.Read(length - (br + readMore));
                    }
                    else
                    {
                        var data = stream.Read(length - br);

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
                            _lastReceivedPacketId = lastPacketId = packetId;
                            packetData = a.Read(dataLength - l);
                        }
                    }
                }
            }

            packet = MCPacketFactory.GetPacket(ConnectionState, packetId);
            //Log.Info($"Got packet: {packet} (0x{packetId:X2})");
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

                // Log.Info($"Received: {packet}");

                packet.Stopwatch.Start();

                using (var memoryStream = new MemoryStream(packetData))
                {
                    using (MinecraftStream minecraftStream = new MinecraftStream(memoryStream, CancellationToken.Token))
                    {
                        packet.Decode(minecraftStream);
                    }
                }

                HandlePacket(packet);

                packet.Stopwatch.Stop();

                if (packet.Stopwatch.ElapsedMilliseconds > 120)
                {
                    Log.Warn(
                        $"Packet handling took too long: {packet.GetType()} | {packet.Stopwatch.ElapsedMilliseconds}ms Processed bytes: {packetData.Length} (Queue size: 0)");
                }

                return true;
            }
            finally
            {
                packet?.PutPool();
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
                // Log.Warn($"Cannot send packet, adding has completed.");
                return;
            }

            if (packet.PacketId == -1) throw new Exception();

            //lock (_readLock)
            {
                PacketWriteQueue.Add(new EnqueuedPacket(packet, CompressionEnabled));
            }
        }

        private MinecraftStream _writerStream;
        private ConnectionState _connectionState;

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
                }

                encodedPacket = ms.ToArray();
            }

            if (enqueued.CompressionEnabled)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (MinecraftStream mc = new MinecraftStream(ms, CancellationToken.Token))
                    {

                        if (encodedPacket.Length >= CompressionThreshold)
                        {
                            //byte[] compressed;
                            //CompressData(encodedPacket, out compressed);

                            mc.WriteVarInt(encodedPacket.Length);

                            using (ZlibStream outZStream = new ZlibStream(
                                mc, CompressionMode.Compress, CompressionLevel.Default, true))
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
                    }

                    encodedPacket = ms.ToArray();
                }
            }

            return encodedPacket;
        }

        private bool SocketConnected(Socket s)
        {
            try
            {
                if (s == null)
                { return false; }

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

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

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
            _writerStream?.Dispose();
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
            while (collection.TryTake(out var _, 0)) { };
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
