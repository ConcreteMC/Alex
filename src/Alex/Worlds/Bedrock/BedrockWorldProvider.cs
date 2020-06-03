using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Data;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Entities;
using Alex.Net;
using Alex.Networking.Bedrock.Net.Raknet;
using Alex.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using MathF = System.MathF;
using PlayerLocation = Alex.API.Utils.PlayerLocation;

namespace Alex.Worlds.Bedrock
{
	public class BedrockWorldProvider : WorldProvider
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		public Alex Alex { get; }
		protected BedrockClient Client { get; }

		private System.Threading.Timer _gameTickTimer;
		private IEventDispatcher EventDispatcher { get; }
		public BedrockWorldProvider(Alex alex, IPEndPoint endPoint, PlayerProfile profile, DedicatedThreadPool threadPool,
			out NetworkProvider networkProvider)
		{
			Alex = alex;
			var eventDispatcher = alex.Services.GetRequiredService<IEventDispatcher>();
			EventDispatcher = eventDispatcher;
			
			//Client = new ExperimentalBedrockClient(alex, alex.Services, this, endPoint);
			Client = new BedrockClient(alex, eventDispatcher, endPoint, profile, threadPool, this);
			networkProvider = Client;
			
			EventDispatcher.RegisterEvents(this);
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
		
		private bool _initiated = false;
		private bool _flying = false;
		private PlayerLocation _lastLocation = new PlayerLocation();
        private PlayerLocation _lastSentLocation = new PlayerLocation();
        
        private long _tickTime = 0;
        private long _lastPrioritization = 0;
        private bool _isRealTick = false;
		private void GameTick(object state)
		{
			var isRealTick = _isRealTick;
			_isRealTick = !isRealTick;
			
			if (World == null) return;

			if (_initiated)
			{
				if (_isRealTick)
				{
					_tickTime++;
				}

				var p = World.Player;
				if (p != null && p is Player player && Client.HasSpawned)
				{
				//	player.IsSpawned = Spawned;

					if (player.IsFlying != _flying && _isRealTick)
					{
						_flying = player.IsFlying;

						McpeAdventureSettings settings = McpeAdventureSettings.CreateObject();
						settings.flags = GetAdventureFlags(); 
						Client.SendPacket(settings);
						//SendPlayerAbilities(player);
					}
					
					var pos = (PlayerLocation)player.KnownPosition.Clone();
					
					if (pos.DistanceTo(_lastSentLocation) > 0.0f) {
                        Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(pos.X,
	                        pos.Y + Player.EyeLevel, pos.Z, pos.HeadYaw,
	                        pos.Yaw, -pos.Pitch), player.KnownPosition.OnGround);

                        _lastSentLocation = pos;
					}

					if ((pos.DistanceTo(_lastLocation) > 16f || MathF.Abs(pos.HeadYaw - _lastLocation.HeadYaw) >= 10f) && (_tickTime - _lastPrioritization >= 10 && _isRealTick))
					{
						World.ChunkManager.FlagPrioritization();
						
						_lastLocation = pos;
						UnloadChunks(new ChunkCoordinates(pos), Client.ChunkRadius + 3);

						_lastPrioritization = _tickTime;
					}
				}

				if (_isRealTick && _tickTime % 20 == 0 && CustomConnectedPong.CanPing)
				{
					Client.SendPing();
				}


				if (_isRealTick)
				{
					World.Player.OnTick();
					World.EntityManager.Tick();
				}
			}
		}

		//private ThreadSafeList<ChunkCoordinates> _loadedChunks = new ThreadSafeList<ChunkCoordinates>();
		private void UnloadChunks(ChunkCoordinates center, double maxViewDistance)
		{
			//var chunkPublisher = Client.LastChunkPublish;
			
			//Client.ChunkRadius
			foreach (var chunk in World.ChunkManager.GetAllChunks())
			{
				if (chunk.Key.DistanceTo(center) > maxViewDistance)
				{
					//_chunkCache.TryRemove(chunkColumn.Key, out var waste);
					UnloadChunk(chunk.Key);
				}
			}
			//Parallel.ForEach(_loadedChunks.ToArray(), (chunkColumn) =>
			//{
				/*if (chunkPublisher != null)
				{
					if (chunkColumn.DistanceTo(new ChunkCoordinates(new Vector3(chunkPublisher.coordinates.X,
						    chunkPublisher.coordinates.Y, chunkPublisher.coordinates.Z))) < chunkPublisher.radius)
						return;
				}*/
				
				
		//	});
		}

		public void UnloadChunk(ChunkCoordinates coordinates)
		{
			World.UnloadChunk(coordinates);
		}

		protected override void Initiate(out LevelInfo info)
		{
			info = new LevelInfo();
			_initiated = true;
			Client.World = World;
			//if (WorldReceiver.Player is Player player)
			//{
			//	WorldReceiver?.UpdatePlayerPosition();
			//}

			CustomConnectedPong.CanPing = true;
			_gameTickTimer = new System.Threading.Timer(GameTick, null, 50, 25);
		}

		private bool VerifyConnection()
		{
			if (Client is BedrockClient c)
			{
				return c.IsConnected;
			}
			
			return true;
		}

		public override Task Load(ProgressReport progressReport)
		{
			Client.GameStarted = false;
			
			return Task.Run(() =>
			{
				Stopwatch timer = Stopwatch.StartNew();
				progressReport(LoadingState.ConnectingToServer, 25);

				var resetEvent = new ManualResetEventSlim(false);
				
				Client.Start(resetEvent);
				progressReport(LoadingState.ConnectingToServer, 50);

			//	Client.HaveServer = true;

				//Client.SendOpenConnectionRequest1();
				if (!resetEvent.Wait(TimeSpan.FromSeconds(5)))
				{
					Client.ShowDisconnect("Could not connect to server!");
					return;
				}

				progressReport(LoadingState.ConnectingToServer, 98);

				//progressReport(LoadingState.LoadingChunks, 0);

				var percentage = 0;
				var statusChanged = false;
				var done = false;
				int previousPercentage = 0;
				bool hasSpawnChunk = false;
				
				while (true)
				{
					double radiusSquared = Math.Pow(Client.ChunkRadius, 2);
					var target = radiusSquared;
					
					percentage = (int)((100 / target) * World.ChunkManager.ChunkCount);
					
					if (Client.GameStarted && percentage != previousPercentage)
					{
						progressReport(LoadingState.LoadingChunks, percentage);
						previousPercentage = percentage;
						
						//Log.Info($"Progress: {percentage} ({ChunksReceived} of {target})");
					}
					
					if (!statusChanged)
					{
						if (Client.PlayerStatus == 3 || Client.PlayerStatusChanged.WaitOne(50) || Client.HasSpawned || Client.ChangeDimensionResetEvent.WaitOne(5))
						{
							statusChanged = true;
							
							//Client.SendMcpeMovePlayer();
				
							
							//Client.IsEmulator = false;
						}
					}
					
					if (!hasSpawnChunk)
					{
						if (World.ChunkManager.TryGetChunk(new ChunkCoordinates(new PlayerLocation(Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z)), out _))
						{
							hasSpawnChunk = true;
						}
					}

					if (((percentage >= 100 || hasSpawnChunk)))
					{
						if (statusChanged)
						{
							break;
						}
					}

					if (!VerifyConnection())
					{
						Client.ShowDisconnect("Connection lost.");
						
						timer.Stop();
						return;
					}
				}
				
				if (World.Player is Player player)
				{
					var packet = McpeSetLocalPlayerAsInitializedPacket.CreateObject();
					packet.runtimeEntityId = Client.EntityId;
					
					Client.SendPacket(packet);
							
					var p = player.KnownPosition;
					Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(p.X, p.Y, p.Z, p.HeadYaw, p.Yaw, p.Pitch), player.KnownPosition.OnGround);
				}

				//SkyLightCalculations.Calculate(WorldReceiver as World);
				
				//Client.IsEmulator = false;
				progressReport(LoadingState.Spawning, 99);
				timer.Stop();
				
				//TODO: Check if spawn position is safe.
			});
		}
		public override void Dispose()
		{
			base.Dispose();
			Client.Dispose();
			
			EventDispatcher?.UnregisterEvents(this);
		}
	}
}
