using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Services;
using Alex.API.World;
using Alex.Gui.Forms;
using Alex.Net;
using Alex.Net.Bedrock.Raknet;
using Alex.Utils.Inventories;
using Alex.Worlds.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using MathF = System.MathF;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.API.Utils.PlayerLocation;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockWorldProvider : WorldProvider, ITicked
	{
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		public Alex Alex { get; }
		protected BedrockClient Client { get; }
		public BedrockFormManager FormManager { get; }
		
		public BedrockWorldProvider(Alex alex, IPEndPoint endPoint, PlayerProfile profile,
			out NetworkProvider networkProvider)
		{
			Alex = alex;
			
			//Client = new ExperimentalBedrockClient(alex, alex.Services, this, endPoint);
			Client = new BedrockClient(alex, endPoint, profile, this);
			networkProvider = Client;

			var guiManager = Alex.GuiManager;
			FormManager = new BedrockFormManager(networkProvider, guiManager, alex.InputManager);
		}

		public override Vector3 GetSpawnPoint()
		{
			return new Vector3(Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z);
		}

		private uint GetAdventureFlags()
		{
			uint flags = 0;

			if (_flying) flags |= 0x200;

			return flags;
		}
		
		private bool _initiated = false;
		private bool _flying = false;
		private PlayerLocation _lastLocation = new PlayerLocation();
        private PlayerLocation _lastSentLocation = new PlayerLocation();
        
        private long _tickTime = 0;
        private long _lastPrioritization = 0;
        public void OnTick()
		{
			if (World == null) return;

			if (_initiated)
			{
				_tickTime++;

				if (World.Player != null && World.Player.IsSpawned && _gameStarted)
				{
					//	player.IsSpawned = Spawned;

					if (World.Player.IsFlying != _flying)
					{
						_flying = World.Player.IsFlying;

						McpeAdventureSettings settings = McpeAdventureSettings.CreateObject();
						settings.flags = GetAdventureFlags();
						Client.SendPacket(settings);
						//SendPlayerAbilities(player);
					}

					var pos = (PlayerLocation) World.Player.KnownPosition.Clone();

					if (pos.DistanceTo(_lastSentLocation) > 0.0f
					    || MathF.Abs(pos.HeadYaw - _lastSentLocation.HeadYaw) > 0.0f
					    || MathF.Abs(pos.Pitch - _lastSentLocation.Pitch) > 0.0f)
					{
						SendLocation(pos);
						_lastSentLocation = pos;
					}

					if ((pos.DistanceTo(_lastLocation) > 16f || MathF.Abs(pos.HeadYaw - _lastLocation.HeadYaw) >= 5.0f)
					    && (_tickTime - _lastPrioritization >= 10))
					{
					//	World.ChunkManager.FlagPrioritization();

						SendLocation(pos);

						_lastLocation = pos;
						
						UnloadChunks(new ChunkCoordinates(pos), Client.ChunkRadius + 3);

						_lastPrioritization = _tickTime;
					}
				}

				//World.Player.OnTick();
				//World.EntityManager.Tick();
				//World.PhysicsEngine.Tick();
			}
		}

		private void SendLocation(PlayerLocation location)
		{
			Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(location.X,
				location.Y + Player.EyeLevel, location.Z, -location.HeadYaw,
				
				- location.Yaw, location.Pitch), location.OnGround);
		}

		//private ThreadSafeList<ChunkCoordinates> _loadedChunks = new ThreadSafeList<ChunkCoordinates>();
		private void UnloadChunks(ChunkCoordinates center, double maxViewDistance)
		{
			var chunkPublisher = Client.LastChunkPublish;

			ChunkCoordinates publisherCenter = center;

			if (chunkPublisher != null)
			{
				publisherCenter = new ChunkCoordinates(
					new Vector3(
						chunkPublisher.coordinates.X, chunkPublisher.coordinates.Y, chunkPublisher.coordinates.Z));
			}

			//Client.ChunkRadius
			foreach (var chunk in World.ChunkManager.GetAllChunks())
			{
				if (chunkPublisher != null)
				{
					if (chunk.Key.DistanceTo(publisherCenter) < (chunkPublisher.radius / 16f))
						continue;
				}
				
				var distance = chunk.Key.DistanceTo(center);
				if (distance > maxViewDistance)
				{
					//_chunkCache.TryRemove(chunkColumn.Key, out var waste);
					World.UnloadChunk(chunk.Key);
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

		protected override void Initiate()
		{
			_initiated = true;
			Client.World = World;
			//World.Player.SetInventory(new BedrockInventory(46));

			//CustomConnectedPong.CanPing = true;
			World.Ticker.RegisterTicked(this);
		}

		public override LoadResult Load(ProgressReport progressReport)
		{
			Client.GameStarted = false;
			
			Stopwatch timer = Stopwatch.StartNew();
			progressReport(LoadingState.ConnectingToServer, 25);
			
			progressReport(LoadingState.ConnectingToServer, 50, "Establishing a connection...");

			if (!Client.Start(TimeSpan.FromSeconds(30)))
			{
				Log.Warn($"Failed to connect to server, resetevent not triggered.");
				
				return LoadResult.Timeout;
			}

			//	Client.HaveServer = true;

			progressReport(LoadingState.ConnectingToServer, 98, "Waiting on server confirmation...");

			//progressReport(LoadingState.LoadingChunks, 0);

			var  percentage         = 0;

			Stopwatch sw = Stopwatch.StartNew();
			
			bool         outOfOrder   = false;
			LoadingState state        = LoadingState.ConnectingToServer;
			string       subTitle     = "";

			while (Client.IsConnected && Client.DisconnectReason != DisconnectReason.Unknown)
			{
				if (Client.Connection.IsNetworkOutOfOrder && !outOfOrder)
				{
					subTitle = "Waiting for network to catch up...";
					outOfOrder = true;
				}
				else if (!Client.Connection.IsNetworkOutOfOrder && outOfOrder)
				{
					subTitle = "";
					outOfOrder = false;
					sw.Restart();
				}

				double radiusSquared = Math.Pow(Client.ChunkRadius, 2);
				var    target        = radiusSquared;

				percentage = (int) ((100 / target) * World.ChunkManager.ChunkCount);
				progressReport(state, percentage, subTitle);

				if (Client.CanSpawn && Client.GameStarted)
				{
					break;
				}

				if (Client.Connection.IsNetworkOutOfOrder)
				{
					subTitle = "Waiting for the network to catch up...";
				}
				else
				{
					if (!Client.GameStarted)
					{
						subTitle = "Waiting on game start...";
					}
					else
					{
						subTitle = "Waiting on spawn confirmation...";
					}
				}

				state = percentage >= 100 ? LoadingState.Spawning : LoadingState.LoadingChunks;

				//Log.Warn($"Status: {statusChanged} | Gamestarted: {Client.GameStarted} | OutOfOrder: {Client.Connection.IsNetworkOutOfOrder}");


				if ((!Client.GameStarted || percentage == 0) && sw.ElapsedMilliseconds >= 15000)
				{
					if (Client.DisconnectReason == DisconnectReason.Kicked)
					{
						return LoadResult.Aborted;
					}

					Log.Warn($"Failed to connect to server, timed-out.");

					return LoadResult.Timeout;
				}
			}

			if (!Client.IsConnected)
				return LoadResult.Timeout;

			var p = World.Player.KnownPosition;

			Client.SendMcpeMovePlayer(
				new MiNET.Utils.PlayerLocation(p.X, p.Y, p.Z, p.HeadYaw, p.Yaw, p.Pitch),
				World.Player.KnownPosition.OnGround);

			timer.Stop();

			World.Player.IsSpawned = true;
			_gameStarted = true;
			
			return LoadResult.Done;
		}

		private bool _gameStarted = false;

		public override void Dispose()
		{
			//World?.Ticker?.UnregisterTicked(this);
			
			base.Dispose();
			Client.Dispose();
		}
	}
}
