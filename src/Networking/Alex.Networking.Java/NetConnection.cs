#region Imports

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Alex.Networking.Java.Events;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Login;
using Alex.Networking.Java.Util;
using NLog;
using Org.BouncyCastle.Utilities.Zlib;

#endregion

namespace Alex.Networking.Java
{
	public class NetConnection : IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(NetConnection));

		private CancellationTokenSource CancellationToken { get; }
		private TcpClient Client { get; set; }
		public IPacketHandler PacketHandler { get; set; } = new DefaultPacketHandler();
		private IPEndPoint TargetEndpoint { get; }
		private BufferBlock<Packet> _packetQueue;

		public NetConnection(IPEndPoint targetEndpoint, CancellationToken cancellationToken)
		{
			TargetEndpoint = targetEndpoint;

			CancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			ConnectionState = ConnectionState.Handshake;
			IsConnected = true;
		}

		public EventHandler<ConnectionClosedEventArgs> OnConnectionClosed;

		private ConnectionState _connectionState;

		public ConnectionState ConnectionState
		{
			get => _connectionState;
			set
			{
				_connectionState = value;

				if (value == ConnectionState.Play) { }
			}
		}

		public long PacketsIn;
		public long PacketsOut;
		public long PacketSizeIn;
		public long PacketSizeOut;
		public bool CompressionEnabled { get; set; }
		public int CompressionThreshold = 256;

		public bool IsConnected { get; private set; }
		public bool LogExceptions { get; set; } = true;

		public DateTime StartTime { get; private set; } = DateTime.UtcNow;
		public long Latency { get; set; } = 0;

		private Thread _readThread;

		public bool Initialize(CancellationToken cancellationToken)
		{
			try
			{
				if (Client != null)
					return false;

				Client = new TcpClient();

				Client.Connect(TargetEndpoint.Address, TargetEndpoint.Port);

				if (!Client.Connected)
					return false;

				_networkStream = new MinecraftStream(Client.GetStream(), CancellationToken.Token);
				_readThread = new Thread(ProcessNetworkRead) { Name = "MC:Java Network Read" };

				_readThread.Start();

				var blockOptions = new ExecutionDataflowBlockOptions
				{
					CancellationToken = CancellationToken.Token, EnsureOrdered = true, MaxDegreeOfParallelism = 1
				};

				_packetQueue = new BufferBlock<Packet>(blockOptions);

				var sendPacketBlock = new ActionBlock<Packet>(WriteNetwork, blockOptions);
				var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
				_packetQueue.LinkTo(sendPacketBlock, linkOptions);
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

			_semaphore?.Release();

			try
			{
				if (CancellationToken.IsCancellationRequested) return;
				CancellationToken.Cancel();

				if (SocketConnected(Client?.Client))
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

		private MinecraftStream _networkStream;

		private int _lastReceivedPacketId;
		private int _lastSentPacketId;
		private int[] _lastSentPacketIds = new int[10];

		private async void ProcessNetworkRead()
		{
			try
			{
				while (!CancellationToken.IsCancellationRequested)
				{
					if (CancellationToken.IsCancellationRequested)
						break;

					var stream = _networkStream;

					if (stream == null)
						break;

					await _semaphore.WaitAsync(CancellationToken.Token);

					try
					{
						if (stream.DataAvailable)
						{
							await TryReadPacketAsync(stream);
						}
					}
					finally
					{
						_semaphore.Release();
					}
				}
			}
			catch (Exception ex)
			{
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

		private async void WriteNetwork(Packet packet)
		{
			try
			{
				var stream = _networkStream;

				try
				{
					var data = await EncodePacketAsync(packet);

					if (data.Length == 0)
						return;

					Interlocked.Increment(ref PacketsOut);
					Interlocked.Add(ref PacketSizeOut, data.Length);

					await stream.WriteVarIntAsync(data.Length);
					await stream.WriteAsync(data);

					if (packet is EncryptionResponsePacket)
					{
						// lock (_readLock)
						await _semaphore.WaitAsync();

						try
						{
							_networkStream = new AesStream(Client.GetStream(), _sharedKey);
						}
						finally
						{
							_semaphore.Release();
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

					copy[^1] = packet.PacketId;
					_lastSentPacketIds = copy;

					_lastSentPacketId = packet.PacketId;
					packet.PutPool();
				}
			}
			catch (Exception ex)
			{
				if (LogExceptions)
				{
					Log.Error(
						ex,
						$"Failed to send packet. Id=0x{packet.PacketId:X2} Type={packet.GetType().ToString()} (Last Received=0x{_lastReceivedPacketId:X2} Last Sent=0x{_lastSentPacketId:X2} ConnectionState={ConnectionState})");

					for (int i = 0; i < _lastSentPacketIds.Length; i++)
					{
						Log.Debug($"Sent: 0x{_lastSentPacketIds[i]:X2}");
					}
				}

				Disconnected(false);
			}
			finally { }
		}

		private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

		private async Task<bool> TryReadPacketAsync(MinecraftStream stream)
		{
			Packets.Packet packet = null;
			int packetId;
			byte[] packetData;


			int length = await stream.ReadVarIntAsync();

			if (length == -1)
				return false;

			if (!CompressionEnabled)
			{
				int packetIdLength;
				packetId = await stream.ReadVarIntAsync();
				packetIdLength = GetVarintLength(packetId);
				_lastReceivedPacketId = packetId;

				if (length - packetIdLength > 0)
				{
					packetData = await stream.ReadAsync(length - packetIdLength);
				}
				else
				{
					packetData = new byte[0];
				}
			}
			else
			{
				int br;
				int dataLength = await stream.ReadVarIntAsync();
				br = GetVarintLength(dataLength);

				if (dataLength == 0)
				{
					packetId = await stream.ReadVarIntAsync();
					_lastReceivedPacketId = packetId;
					packetData = await stream.ReadAsync(length - (br + GetVarintLength(packetId)));
				}
				else
				{
					var data = await stream.ReadAsync(length - br);
					
					using (MinecraftStream a = new MinecraftStream(CancellationToken.Token))
					{
						using (var outZStream = new ZInputStream(new MemoryStream(data)))
						{
							await outZStream.CopyToAsync(a);
						}

						a.Position = 0;

						packetId = await a.ReadVarIntAsync();
						_lastReceivedPacketId = packetId;
						packetData = await a.ReadAsync(dataLength - GetVarintLength(packetId));
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

				packet.Stopwatch.Start();

				using (var memoryStream = new MemoryStream(packetData))
				{
					using (MinecraftStream minecraftStream = new MinecraftStream(memoryStream, CancellationToken.Token))
					{
						await packet.DecodeAsync(minecraftStream);
					}
				}

				await HandlePacket(packet);

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
				if (packet?.IsPooled == true)
				{
					packet?.PutPool();
				}
			}
		}

		private int GetVarintLength(int val)
		{
			int amount = 0;

			do
			{
				val >>= 7;
				amount++;
			} while (val != 0);

			return amount;
		}

		private Dictionary<ConnectionState, ConcurrentDictionary<int, int>> UnhandledPacketsFilter =
			new Dictionary<ConnectionState, ConcurrentDictionary<int, int>>()
			{
				{ ConnectionState.Handshake, new ConcurrentDictionary<int, int>() },
				{ ConnectionState.Status, new ConcurrentDictionary<int, int>() },
				{ ConnectionState.Login, new ConcurrentDictionary<int, int>() },
				{ ConnectionState.Play, new ConcurrentDictionary<int, int>() },
			};


		protected virtual async Task HandlePacket(Packets.Packet packet)
		{
			switch (ConnectionState)
			{
				case ConnectionState.Handshake:
					await PacketHandler.HandleHandshake(packet);

					break;

				case ConnectionState.Status:
					await PacketHandler.HandleStatus(packet);

					break;

				case ConnectionState.Login:
					await PacketHandler.HandleLogin(packet);

					break;

				case ConnectionState.Play:
					await PacketHandler.HandlePlay(packet);

					break;
			}
		}

		public async void SendPacket(Packet packet)
		{
			var packetQueue = this._packetQueue;

			if (packet == null || packetQueue == null)
				return;

			await packetQueue.SendAsync(packet);
		}

		private async Task<byte[]> EncodePacketAsync(Packet enqueued)
		{
			var packet = enqueued;
			byte[] encodedPacket;

			using (MemoryStream ms = new MemoryStream())
			{
				using (MinecraftStream mc = new MinecraftStream(ms, CancellationToken.Token))
				{
					await mc.WriteVarIntAsync(packet.PacketId);
					await packet.EncodeAsync(mc);
				}

				encodedPacket = ms.ToArray();
			}

			if (CompressionEnabled)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					using (MinecraftStream mc = new MinecraftStream(ms, CancellationToken.Token))
					{
						if (encodedPacket.Length >= CompressionThreshold)
						{
							await mc.WriteVarIntAsync(encodedPacket.Length);

							using (var outZStream = new ZOutputStream(
								       mc, true))
							{
								await outZStream.WriteAsync(encodedPacket, 0, encodedPacket.Length);
							}
						}
						else //Uncompressed
						{
							await mc.WriteVarIntAsync(0);
							await mc.WriteAsync(encodedPacket);
						}
					}

					encodedPacket = ms.ToArray();
				}
			}

			return encodedPacket;
		}

		private bool SocketConnected(Socket s)
		{
			if (s == null)
				return false;
			
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

		private bool _disposed = false;

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			Stop();

			_packetQueue?.Complete();
			_packetQueue = null;

			_semaphore?.Dispose();
			_semaphore = null;

			CancellationToken?.Dispose();

			try
			{
				_networkStream?.Dispose();
				_networkStream = null;

				// _writerStream?.Dispose();
				Client?.Dispose();
				Client = null;
			}
			catch (Exception ex)
			{
				Log.Warn(ex, $"Could not dispose.");
			}

			foreach (var state in UnhandledPacketsFilter.ToArray())
			{
				foreach (var p in state.Value)
				{
					Log.Warn(
						$"({state.Key}) unhandled: 0x{p.Key:X2} ({(state.Key == ConnectionState.Play ? MCPacketFactory.GetPlayPacketName(p.Key) : "Unknown")}) * {p.Value}");
				}
			}

			UnhandledPacketsFilter.Clear();
		}
	}
}