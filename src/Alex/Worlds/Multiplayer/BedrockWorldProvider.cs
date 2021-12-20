using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Alex.Common.Services;
using Alex.Common.World;
using Alex.Gui.Forms;
using Alex.Net;
using Alex.Net.Bedrock;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Multiplayer.Bedrock;
using Alex.Worlds.Multiplayer.Bedrock.Resources;
using Microsoft.Xna.Framework;
using MiNET.Net;
using NLog;
using ChunkCoordinates = Alex.Common.Utils.Vectors.ChunkCoordinates;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.Common.Utils.Vectors.PlayerLocation;

namespace Alex.Worlds.Multiplayer
{
	public class BedrockWorldProvider : WorldProvider
	{
		private readonly IPEndPoint _endPoint;
		private readonly PlayerProfile _profile;
		private static Logger Log = LogManager.GetCurrentClassLogger();
		
		public Alex Alex { get; }
		protected BedrockClient Client { get; set; }
		public BedrockFormManager FormManager { get; set; }
		
		public BedrockWorldProvider(Alex alex, IPEndPoint endPoint, PlayerProfile profile)
		{
			_endPoint = endPoint;
			_profile = profile;
			Alex = alex;
			
			//Client = new ExperimentalBedrockClient(alex, alex.Services, this, endPoint);
		}

		public void Init(out NetworkProvider networkProvider)
		{
			Client = GetClient(Alex, _endPoint, _profile);
			networkProvider = Client;

			var guiManager = Alex.GuiManager;
			FormManager = new BedrockFormManager(networkProvider, guiManager, Alex.InputManager);
		}

		protected virtual BedrockClient GetClient(Alex alex, IPEndPoint endPoint, PlayerProfile profile)
		{
			return new BedrockClient(alex, endPoint, profile, this);
		}
		
		public override Vector3 GetSpawnPoint()
		{
			return World?.SpawnPoint ?? Vector3.Zero;
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
        
        private long _tickTime = 0;
        private long _lastPrioritization = 0;
        private long _serverTick = 0;
        public override void OnTick()
		{
			if (World == null) return;

			if (_initiated)
			{
				_serverTick++;
				_tickTime++;

				if (World.Player != null && World.Player.IsSpawned && _gameStarted)
				{
					if (World.Player.IsFlying != _flying)
					{
						_flying = World.Player.IsFlying;

						McpeAdventureSettings settings = McpeAdventureSettings.CreateObject();
						settings.flags = GetAdventureFlags();
						Client.SendPacket(settings);
					}

					var pos = (PlayerLocation) World.Player.KnownPosition.Clone();

					if ((pos.DistanceTo(_lastLocation) >= 16f)
					    && (_tickTime - _lastPrioritization >= 10))
					{
						_lastLocation = pos;
						
						UnloadChunks(new ChunkCoordinates(pos), Client.World.ChunkManager.RenderDistance + 2);
						_lastPrioritization = _tickTime;
					}
					
					SendLocation(World.Player.RenderLocation);
				}
			}
		}

		private void SendLocation(PlayerLocation location)
		{
			Client.SendMcpeMovePlayer(new PlayerLocation(location.X, location.Y + Player.EyeLevel, location.Z, -location.HeadYaw,-location.Yaw, location.Pitch)
			{
				OnGround = location.OnGround
			}, 0, World.Time);
		}
		
		private void UnloadChunks(ChunkCoordinates center, double maxViewDistance)
		{
			//var chunkPublisher = Client.ChunkPublishCenter;

			ChunkCoordinates publisherCenter = new ChunkCoordinates(Client.ChunkPublisherPosition);

			foreach (var chunk in World.ChunkManager.GetAllChunks())
			{
				if (chunk.Key.DistanceTo(publisherCenter) <= (Client.ChunkPublisherRadius / 16f))
					continue;

				var distance = chunk.Key.DistanceTo(center);
				if (distance > maxViewDistance)
				{
					World.UnloadChunk(chunk.Key);
				}
			}
		}

		protected override void Initiate()
		{
			_initiated = true;
			Client.World = World;
			Client.CommandProvider = new BedrockCommandProvider(World);
		}

		public override LoadResult Load(ProgressReport progressReport)
		{
			Client.GameStarted = false;
			
			Stopwatch timer = Stopwatch.StartNew();
			progressReport(LoadingState.ConnectingToServer, 25);
			
			progressReport(LoadingState.ConnectingToServer, 50, "Establishing a connection...");

			CancellationTokenSource cts = new CancellationTokenSource();
			cts.CancelAfter(TimeSpan.FromSeconds(30));
			if (!Client.Start(cts.Token))
			{
				Log.Warn($"Failed to connect to server, resetevent not triggered.");
				
				return LoadResult.Timeout;
			}

			progressReport(LoadingState.ConnectingToServer, 98, "Waiting on server confirmation...");

			var  percentage         = 0;

			Stopwatch sw = Stopwatch.StartNew();
			
			bool         outOfOrder   = false;
			LoadingState state        = LoadingState.ConnectingToServer;
			string       subTitle     = "";

			bool waitingOnResources = false;
			bool loadingResources = false;
			
			var resourcePackManager = Client?.ResourcePackManager;
			void statusHandler (object sender, ResourcePackManager.ResourceStatusChangedEventArgs args)
			{
				switch (args.Status)
				{
					case ResourcePackManager.ResourceManagerStatus.Ready:
						waitingOnResources = false;
						loadingResources = false;
						break;
					case ResourcePackManager.ResourceManagerStatus.ReceivingResources:
						waitingOnResources = true;
						loadingResources = false;
						break;
					case ResourcePackManager.ResourceManagerStatus.StartLoading:
						waitingOnResources = false;
						loadingResources = true;
						break;
					case ResourcePackManager.ResourceManagerStatus.FinishedLoading:
						waitingOnResources = false;
						loadingResources = false;
						break;
				}
			}

			if (resourcePackManager != null)
			{
				resourcePackManager.StatusChanged += statusHandler;

				waitingOnResources = resourcePackManager.Status != ResourcePackManager.ResourceManagerStatus.Ready
				                     && resourcePackManager.Status
				                     != ResourcePackManager.ResourceManagerStatus.Initialized;
			}

			while (Client.IsConnected && Client.DisconnectReason != DisconnectReason.Unknown)
			{
				progressReport(state, percentage, subTitle);

				if (waitingOnResources)
				{
					state = LoadingState.RetrievingResources;
					percentage = (int) Math.Ceiling(resourcePackManager.Progress * 100);
				}
				else if (loadingResources)
				{
					state = LoadingState.LoadingResources;
					percentage = resourcePackManager.LoadingProgress;
				}
				else if (!Client.Connection.IsNetworkOutOfOrder && !outOfOrder)
				{
					double radiusSquared = Math.Pow(Client.World.ChunkManager.RenderDistance, 2);
					var target = radiusSquared;
					percentage = (int) ((100 / target) * World.ChunkManager.ChunkCount);

					state = percentage >= 100 ? LoadingState.Spawning : LoadingState.LoadingChunks;

					if (!Client.GameStarted)
					{
						subTitle = "Waiting on game start...";
					}
					else
					{
						subTitle = "Waiting on spawn confirmation...";
					}
				}

				if (Client.Connection.IsNetworkOutOfOrder)
				{
					if (!outOfOrder)
					{
						subTitle = "Waiting for network to catch up...";
						outOfOrder = true;
					}
				}
				else
				{
					if (outOfOrder)
					{
						subTitle = "";
						outOfOrder = false;
						sw.Restart();
					}
				}

				if (Client.CanSpawn && Client.GameStarted && !waitingOnResources && !loadingResources)
				{
					break;
				}

				if ((!Client.GameStarted || percentage == 0) && sw.ElapsedMilliseconds >= 15000)
				{
					if (Client.DisconnectReason == DisconnectReason.Kicked)
					{
						return LoadResult.Kicked;
					}

					Log.Warn($"Failed to connect to server, timed-out.");

					return LoadResult.Timeout;
				}
			}

			if (!Client.IsConnected)
				return LoadResult.Timeout;

			Client.MarkAsInitialized();

			timer.Stop();

			World.Player.OnSpawn();
			OnSpawn();
			
			_gameStarted = true;
			
			if (resourcePackManager != null)
				resourcePackManager.StatusChanged -= statusHandler;

			progressReport(LoadingState.Spawning, 99, "Waiting for Spawn Chunk");
			SpinWait.SpinUntil(() => !World.Player.WaitingOnChunk);
			return LoadResult.Done;
		}

		protected virtual void OnSpawn()
		{
			
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
