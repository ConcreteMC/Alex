using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Alex.API.Data;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.World;
using Alex.Entities;
using Alex.Utils;
using Microsoft.Xna.Framework;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using PlayerLocation = Alex.API.Utils.PlayerLocation;

namespace Alex.Worlds.Bedrock
{
	public class BedrockWorldProvider : WorldProvider
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		public Alex Alex { get; }
		private BedrockClient Client { get; }

		private System.Threading.Timer _gameTickTimer;
		public BedrockWorldProvider(Alex alex, IPEndPoint endPoint, PlayerProfile profile,
			out INetworkProvider networkProvider)
		{
			Alex = alex;

			Client = new BedrockClient(alex, endPoint, profile, new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount, ThreadType.Background, "BedrockClientThread")), this);
			networkProvider = Client;
		}

		public override Vector3 GetSpawnPoint()
		{
			return new Vector3(Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z);
		}

		private uint GetAdventureFlags()
		{
			uint flags = 0;
			//if (IsWorldImmutable || player.Gamemode == Gamemode.Adventure) flags |= 0x01; // Immutable World (Remove hit markers client-side).
			//if (IsNoPvp || player. || player.Gamemode == Gamemode.Spectator) flags |= 0x02; // No PvP (Remove hit markers client-side).
			//if (IsNoPvm || IsSpectator || player.Gamemode == Gamemode.Spectator) flags |= 0x04; // No PvM (Remove hit markers client-side).
			//if (IsNoMvp || IsSpectator || player.Gamemode == Gamemode.Spectator) flags |= 0x08;

			//if (IsAutoJump) flags |= 0x20;

			//if (AllowFly || GameMode == GameMode.Creative) flags |= 0x40;

			//if (IsNoClip || IsSpectator || GameMode == GameMode.Spectator) flags |= 0x80; // No clip

			//if (IsWorldBuilder) flags |= 0x100; // Worldbuilder

			if (_flying) flags |= 0x200;

			return flags;
		}

		private object _entityTicks = new object();
		private bool _isSneaking = false;
		private bool _isSprinting = false;
		private bool _initiated = false;
		private bool _flying = false;
		private PlayerLocation _lastLocation = new PlayerLocation();
        private PlayerLocation _lastSentLocation = new PlayerLocation();
        private Stopwatch _stopwatch = Stopwatch.StartNew();
		private void GameTick(object state)
		{
			if (WorldReceiver == null) return;

			if (_initiated)
			{
				
				var p = WorldReceiver.GetPlayerEntity();
				if (p != null && p is Player player && Client.HasSpawned)
				{
				//	player.IsSpawned = Spawned;

					if (player.IsFlying != _flying)
					{
						_flying = player.IsFlying;

						McpeAdventureSettings settings = McpeAdventureSettings.CreateObject();
						settings.flags = GetAdventureFlags(); 
						Client.SendPacket(settings);
						//SendPlayerAbilities(player);
					}
					
					var pos = (PlayerLocation)player.KnownPosition.Clone();
					Client.CurrentLocation = new MiNET.Utils.PlayerLocation(pos.X,
						pos.Y + Player.EyeLevel, pos.Z, pos.HeadYaw,
						pos.Yaw, -pos.Pitch);

                    if (pos.DistanceTo(_lastSentLocation) > 0.0f) {
                        Client.SendMcpeMovePlayer();
                    }

					if (pos.DistanceTo(_lastLocation) > 16f && _stopwatch.ElapsedMilliseconds > 500)
					{
						_stopwatch.Stop();
						_stopwatch.Reset();
						_lastLocation = pos;
						UnloadChunks(new ChunkCoordinates(pos), Client.ChunkRadius);
						_stopwatch.Restart();
					}
				}
			}
		}

		private ThreadSafeList<ChunkCoordinates> _loadedChunks = new ThreadSafeList<ChunkCoordinates>();
		private void UnloadChunks(ChunkCoordinates center, double maxViewDistance)
		{
			Parallel.ForEach(_loadedChunks.ToArray(), (chunkColumn) =>
			{
				if (chunkColumn.DistanceTo(center) > maxViewDistance)
				{
					//_chunkCache.TryRemove(chunkColumn.Key, out var waste);
					UnloadChunk(chunkColumn);
				}
			});
		}

		public IEnumerable<ChunkCoordinates> LoadedChunks => _loadedChunks.ToArray();
		
		public void UnloadChunk(ChunkCoordinates coordinates)
		{
			UnloadChunk(coordinates.X, coordinates.Z);
			_loadedChunks.Remove(coordinates);
		}

		protected override void Initiate(out LevelInfo info, out IChatProvider chatProvider)
		{
			info = new LevelInfo();
			chatProvider = Client;
			_initiated = true;
			Client.WorldReceiver = WorldReceiver;
			WorldReceiver?.UpdatePlayerPosition(new API.Utils.PlayerLocation(
				new Vector3(Client.CurrentLocation.X, Client.CurrentLocation.Y, Client.CurrentLocation.Z),
				Client.CurrentLocation.HeadYaw, Client.CurrentLocation.Yaw, Client.CurrentLocation.Pitch));

			_gameTickTimer = new System.Threading.Timer(GameTick, null, 50, 50);
		}

		public override Task Load(ProgressReport progressReport)
		{
			return Task.Run(() =>
			{
				progressReport(LoadingState.ConnectingToServer, 25);

				Client.StartClient();
				progressReport(LoadingState.ConnectingToServer, 50);

				Client.HaveServer = true;

				Client.SendOpenConnectionRequest1();
				Client.ConnectionAcceptedWaitHandle.Wait();

				progressReport(LoadingState.ConnectingToServer, 100);

				progressReport(LoadingState.LoadingChunks, 0);

				double radiusSquared = Math.Pow(Client.ChunkRadius, 2);var target = radiusSquared * 3;

				var statusChanged = false;
				while (!statusChanged || !Client.HasSpawned)
				{
					progressReport(LoadingState.LoadingChunks, ((int)(_chunksReceived / target) * 100));

					if (!statusChanged)
					{
						if (Client.PlayerStatusChangedWaitHandle.WaitOne(50))
						{
							statusChanged = true;
							Client.IsEmulator = false;
						}
					}
				}

				//Client.IsEmulator = false;
				progressReport(LoadingState.Spawning, 99);
			});
		}

		private int _chunksReceived = 0;
		public void ChunkReceived(ChunkColumn chunkColumn)
		{
			_chunksReceived++;
			var coords = new ChunkCoordinates(chunkColumn.X, chunkColumn.Z);
			
			//if (!_loadedChunks.Contains(coords))
				_loadedChunks.TryAdd(coords);
			
			//sLog.Info($"Chunk received");
			base.LoadChunk(chunkColumn, chunkColumn.X, chunkColumn.Z, true);
		}
		
		public void ReceivedTabComplete(int transactionId, int start, int length, TabCompleteMatch[] matches)
		{
			
		}

		public IChatReceiver GetChatReceiver => ChatReceiver;

		public override void Dispose()
		{
			base.Dispose();
			Client.Dispose();
		}
	}
}
