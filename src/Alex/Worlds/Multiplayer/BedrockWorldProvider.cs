using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Alex.Common.Services;
using Alex.Common.World;
using Alex.Gui.Forms;
using Alex.Net;
using Alex.Net.Bedrock;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Multiplayer.Bedrock.Resources;
using MiNET.Net;
using NLog;
using ChunkCoordinates = Alex.Common.Utils.Vectors.ChunkCoordinates;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.Common.Utils.Vectors.PlayerLocation;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Alex.Worlds.Multiplayer
{
	public class BedrockWorldProvider : WorldProvider
	{
		public readonly ServerConnectionDetails ConnectionDetails;
		private readonly PlayerProfile _profile;
		private static Logger Log = LogManager.GetCurrentClassLogger();

		public Alex Alex { get; }
		protected BedrockClient Client { get; set; }
		public BedrockFormManager FormManager { get; set; }

		public BedrockWorldProvider(Alex alex, ServerConnectionDetails connectionDetails, PlayerProfile profile)
		{
			ConnectionDetails = connectionDetails;
			_profile = profile;
			Alex = alex;

			//Client = new ExperimentalBedrockClient(alex, alex.Services, this, endPoint);
		}

		public void Init(out NetworkProvider networkProvider)
		{
			Client = GetClient(Alex, ConnectionDetails, _profile);
			networkProvider = Client;

			var guiManager = Alex.GuiManager;
			FormManager = new BedrockFormManager(networkProvider, guiManager, Alex.InputManager);
		}

		protected virtual BedrockClient GetClient(Alex alex, ServerConnectionDetails endPoint, PlayerProfile profile)
		{
			return new BedrockClient(alex, endPoint, profile, this);
		}

		public override Vector3 GetSpawnPoint()
		{
			return World?.SpawnPoint ?? Vector3.Zero;
		}

		private bool _initiated = false;
		private PlayerLocation _lastLocation = new PlayerLocation();

		private long _tickTime = 0;
		private long _lastPrioritization = 0;

		public override void OnTick()
		{
			if (World == null) return;

			if (_initiated)
			{
				_tickTime++;
				Client.Tick++;

				if (World.Player != null && World.Player.IsSpawned && _gameStarted)
				{
					var pos = (PlayerLocation)World.Player.KnownPosition.Clone();

					if ((pos.DistanceTo(_lastLocation) >= 16f) && (_tickTime - _lastPrioritization >= 10))
					{
						_lastLocation = pos;

						UnloadChunks(new ChunkCoordinates(pos), Client.World.ChunkManager.RenderDistance + 2);
						_lastPrioritization = _tickTime;
					}

					SendLocation(World.Player.RenderLocation);
					
					if (_tickTime % 20 == 0)
						Client.ChunkProcessor.RequestMissing();
				}
			}
		}

		private PlayerLocation _previousPlayerLocation = new PlayerLocation();

		private void SendLocation(PlayerLocation location)
		{
			if (Client.ServerAuthoritiveMovement)
			{
				var player = World?.Player;

				if (player == null)
					return;

				var delta = _previousPlayerLocation.ToVector3() - location.ToVector3();

				var inputFlags = player.Controller.InputFlags;
				var heading = player.Controller.GetMoveVector(inputFlags);

				McpePlayerAuthInput input = McpePlayerAuthInput.CreateObject();
				input.InputFlags = inputFlags;
				input.Position = new System.Numerics.Vector3(location.X, location.Y + Player.EyeLevel, location.Z);
				input.Yaw = -location.Yaw;
				input.HeadYaw = -location.HeadYaw;
				input.Pitch = -location.Pitch;
				input.MoveVector = new Vector2(heading.X, heading.Z);
				input.Delta = new System.Numerics.Vector3(delta.X, delta.Y, delta.Z);
				input.Tick = Client.Tick;
				input.PlayMode = McpePlayerAuthInput.PlayerPlayMode.Normal;
				input.InputMode = McpePlayerAuthInput.PlayerInputMode.Mouse;

				Client.SendPacket(input);

				_previousPlayerLocation = location;
			}
			else
			{
				Client.SendMcpeMovePlayer(
					new PlayerLocation(
						location.X, location.Y + Player.EyeLevel, location.Z, -location.HeadYaw, -location.Yaw,
						location.Pitch) { OnGround = location.OnGround }, 0, World.Time);
			}
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
			Client.ChunkProcessor.Init(World);
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

			var percentage = 0;

			Stopwatch sw = Stopwatch.StartNew();

			bool outOfOrder = false;
			LoadingState state = LoadingState.ConnectingToServer;
			string subTitle = "";
			
			var resourcePackManager = Client?.ResourcePackManager;

			/*void statusHandler(object sender, ResourcePackManager.ResourceStatusChangedEventArgs args)
			{
				switch (args.Status)
				{
					case ResourcePackManager.ResourceManagerStatus.Ready:
						waitingOnResources = false;
						loadingResources = false;

						break;

					case ResourcePackManager.ResourceManagerStatus.Downloading:
						waitingOnResources = true;
						loadingResources = false;

						break;

					case ResourcePackManager.ResourceManagerStatus.Loading:
						waitingOnResources = false;
						loadingResources = true;

						break;
					case ResourcePackManager.ResourceManagerStatus.Initialized:
						break;
				}
			}*/

			try
			{
				if (resourcePackManager != null)
				{
				//	resourcePackManager.StatusChanged += statusHandler;
				}

				LoadResult loadResult = LoadResult.Unknown;

				while (Client.IsConnected && Client.DisconnectReason != DisconnectReason.Unknown)
				{
					progressReport(state, percentage, $"{subTitle}");

					if (resourcePackManager != null && resourcePackManager.WaitingOnResources)
					{
						state = LoadingState.RetrievingResources;
						percentage = (int)Math.Ceiling(resourcePackManager.Progress * 100);
					}
					else if (resourcePackManager != null && resourcePackManager.Status == ResourcePackManager.ResourceManagerStatus.Loading)
					{
						state = LoadingState.LoadingResources;
						percentage = resourcePackManager.LoadingProgress;
					}
					else if (!Client.Connection.IsNetworkOutOfOrder && !outOfOrder)
					{
						double radiusSquared = Math.Pow(Client.World.ChunkManager.RenderDistance, 2);
						var target = radiusSquared;
						percentage = (int)((100 / target) * World.ChunkManager.ChunkCount);

						state = percentage >= 100 ? LoadingState.Spawning : LoadingState.LoadingChunks;

						if (!Client.GameStarted)
						{
							subTitle = "Waiting on game start";
						}
						else if (Client.PlayerStatus != McpePlayStatus.PlayStatus.PlayerSpawn)
						{
							subTitle = "Waiting on spawn ";
						}
						else
						{
							subTitle = "Waiting on spawn confirmation";
						}
					}

					if (Client.Connection.IsNetworkOutOfOrder && !outOfOrder)
					{
						subTitle = "Waiting for network to catch up";
						outOfOrder = true;
					}
					else if (!Client.Connection.IsNetworkOutOfOrder && outOfOrder)
					{
						subTitle = "";
						outOfOrder = false;
						sw.Restart();
					}
					
					if (resourcePackManager != null && resourcePackManager.WaitingOnResources)
						sw.Restart();

					if (Client.CanSpawn && Client.GameStarted && (resourcePackManager == null
					                                              || (resourcePackManager.Status
					                                                  == ResourcePackManager.ResourceManagerStatus.Ready
					                                                  || (!resourcePackManager.WaitingOnResources
					                                                      && resourcePackManager.Status
					                                                      == ResourcePackManager.ResourceManagerStatus
						                                                     .Initialized))))
					{
						break;
					}

					if ((!Client.GameStarted || percentage == 0) && sw.ElapsedMilliseconds >= 30000)
					{
						if (Client.DisconnectReason == DisconnectReason.Kicked)
						{
							loadResult = LoadResult.Kicked;

							break;
						}

						Log.Warn($"Failed to connect to server, timed-out.");

						loadResult = LoadResult.Timeout;

						break;
					}
				}
				
				Log.Info($"Exited load loop.");

				if (Client.DisconnectReason == DisconnectReason.Kicked)
					return LoadResult.Kicked;

				if (Client.DisconnectReason == DisconnectReason.ServerOutOfDate
				    || Client.DisconnectReason == DisconnectReason.ClientOutOfDate)
					return LoadResult.VersionMismatch;

				if (!Client.IsConnected)
					return LoadResult.Timeout;

				if (loadResult != LoadResult.Unknown)
				{
					return loadResult;
				}
				
				Client.RequestRenderDistance(0, Client.World.ChunkManager.RenderDistance);

				McpeInteract interact = McpeInteract.CreateObject();
				interact.actionId = (byte) McpeInteract.Actions.MouseOver;
				interact.targetRuntimeEntityId = 0;
				interact.Position = System.Numerics.Vector3.Zero;
				Client.SendPacket(interact);
				
				Client.MarkAsInitialized();

				timer.Stop();

				World.Player.OnSpawn();
				OnSpawn();

				_gameStarted = true;

				progressReport(LoadingState.Spawning, 99, "Waiting for Spawn Chunk");
				//SpinWait.SpinUntil(() => !World.Player.WaitingOnChunk);

				return LoadResult.Done;
			}
			finally
			{
			
			}
		}

		protected virtual void OnSpawn() { }

		private bool _gameStarted = false;

		public override void Dispose()
		{
			//World?.Ticker?.UnregisterTicked(this);
			FormManager?.Dispose();
			FormManager = null;
			
			base.Dispose();
			Client.Dispose();
		}
	}
}