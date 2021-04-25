using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.API;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Services;
using Alex.Gamestates.InGame.Hud;
using Alex.Graphics.Camera;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Items;
using Alex.Net;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Services.Discord;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET.Utils;
using NLog;
using RocketUI;
using GpuResourceManager = Alex.API.Graphics.GpuResourceManager;
using PlayerLocation = Alex.API.Utils.Vectors.PlayerLocation;

namespace Alex.Gamestates.InGame
{
	public class PlayingState : GameState
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(PlayingState));
		
		public World World { get; private set; }

        private WorldProvider WorldProvider { get; set; }
        private NetworkProvider NetworkProvider { get; set; }

		private readonly PlayingHud _playingHud;
		private readonly GuiDebugInfo _debugInfo;
		private readonly NetworkDebugHud _networkDebugHud;
		
		private GuiMiniMap MiniMap { get; }
		private bool RenderMinimap { get; set; } = false;
		public PlayingState(Alex alex, GraphicsDevice graphics, WorldProvider worldProvider, NetworkProvider networkProvider) : base(alex)
		{
			NetworkProvider = networkProvider;

			World = new World(alex.Services, graphics, Options, networkProvider);
			World.Player.IsFirstPersonMode = true;
			
			WorldProvider = worldProvider;
			var title = new TitleComponent();

			WorldProvider = worldProvider;
			WorldProvider.Init(World);
			
			Alex.ParticleManager.Initialize(World.Camera);

			WorldProvider.TitleComponent = title;

			_playingHud = new PlayingHud(Alex, World.Player, title);
			_playingHud.Chat.Network = networkProvider;
			
			WorldProvider.ScoreboardView = _playingHud.Scoreboard;
			WorldProvider.ChatRecipient = _playingHud;
			WorldProvider.BossBarContainer = _playingHud.BossBar;
			//WorldProvider.ScoreboardView
			
			_debugInfo = new GuiDebugInfo();
            InitDebugInfo();
            
            MiniMap = new GuiMiniMap(World.ChunkManager)
            {
	            Anchor = Alignment.TopRight
            };

            var settings = GetService<IOptionsProvider>();
            settings.AlexOptions.VideoOptions.Minimap.Bind(OnMinimapSettingChange);
            RenderMinimap = settings.AlexOptions.VideoOptions.Minimap.Value;
            
            if (RenderMinimap)
            {
	            _playingHud.AddChild(MiniMap);
            }
            
            _networkDebugHud = new NetworkDebugHud(NetworkProvider);
		}

		private void OnMinimapSettingChange(bool oldvalue, bool newvalue)
		{
			RenderMinimap = newvalue;
			if (!newvalue)
			{
				_playingHud.RemoveChild(MiniMap);
			}
			else
			{
				_playingHud.AddChild(MiniMap);
			}
		}

		protected override void OnLoad(IRenderArgs args)
		{
			Alex.InGame = true;
			
			World.SpawnPoint = WorldProvider.GetSpawnPoint();
			World.Camera.MoveTo(World.GetSpawnPoint(), Vector3.Zero);

			base.OnLoad(args);
		}

		protected override void OnShow()
		{
			Alex.IsMouseVisible = false;

			//if (RenderNetworking) 
			Alex.GuiManager.AddScreen(_networkDebugHud);
			
			base.OnShow();
			Alex.GuiManager.AddScreen(_playingHud);

			if (RenderDebug)
				Alex.GuiManager.AddScreen(_debugInfo);
			
			World.Ticker.RegisterTicked(_playingHud.Title);
			_playingHud.Title.Ready();
		}

		protected override void OnHide()
		{
			World.Ticker.UnregisterTicked(_playingHud.Title);
			
			Alex.GuiManager.RemoveScreen(_debugInfo);
			Alex.GuiManager.RemoveScreen(_playingHud);
			Alex.GuiManager.RemoveScreen(_networkDebugHud);
			
			base.OnHide();
		}

		private long _ramUsage = 0;
		private long _threadsUsed, _maxThreads;
		private Biome _currentBiome = BiomeUtils.GetBiomeById(0);
		private int _currentBiomeId = 0;
		private void InitDebugInfo()
		{
			string gameVersion = VersionUtils.GetVersion();
			_debugInfo.AddDebugLeft(
				() =>
				{
					double avg = 0;

				/*	if (World.ChunkManager.TotalChunkUpdates > 0)
					{
						avg = (World.ChunkManager.ChunkUpdateTime / World.ChunkManager.TotalChunkUpdates)
						   .TotalMilliseconds;
					}*/

					return
						$"Alex {gameVersion} ({Alex.FpsMonitor.Value:##} FPS, {World.Ticker.TicksPerSecond:##} TPS, Chunk Updates: {World.EnqueuedChunkUpdates} queued, {World.ConcurrentChunkUpdates} active, {ChunkColumn.AverageUpdateTime:F2}ms avg, {ChunkColumn.MinUpdateTime:F2}ms min, {ChunkColumn.MaxUpdateTime:F2}ms max)";
				}, TimeSpan.FromMilliseconds(50));
			
			_debugInfo.AddDebugLeft(() =>
			{
				var pos = World?.Player?.KnownPosition ?? new PlayerLocation();
				var blockPos = pos.GetCoordinates3D();
				return $"Position: (X={pos.X:F2}, Y={pos.Y:F2}, Z={pos.Z:F2}, OnGround={pos.OnGround}) / Block: ({blockPos.X:D}, {blockPos.Y:D}, {blockPos.Z:D})";
			}, TimeSpan.FromMilliseconds(50));
			
			_debugInfo.AddDebugLeft(() =>
			{
				var pos =  World?.Player?.KnownPosition ?? new PlayerLocation();
				return  $"Facing: {GetCardinalDirection(pos)} (HeadYaw={pos.HeadYaw:F2}, Yaw={pos.Yaw:F2}, Pitch={pos.Pitch:F2})";
			}, TimeSpan.FromMilliseconds(50));
			
			_debugInfo.AddDebugLeft(() =>
			{
				var pos = World?.Player?.Velocity ?? Vector3.Zero;
				return $"Velocity: (X={pos.X:F2}, Y={pos.Y:F2}, Z={pos.Z:F2}) ({World.Player.Movement.MetersPerSecond:F3} m/s)";// / Target Speed: {(World.Player.CalculateMovementSpeed() * 20f):F3} m/s";
			});

			_debugInfo.AddDebugLeft(() => $"Primitives: {Alex.Metrics.PrimitiveCount:N0} Draw count: {Alex.Metrics.DrawCount}", TimeSpan.FromMilliseconds(500));
			_debugInfo.AddDebugLeft(() => $"Textures: {Alex.Metrics.TextureCount:N0} Sprite count: {Alex.Metrics.SpriteCount}", TimeSpan.FromMilliseconds(500));
		//	_debugInfo.AddDebugLeft(() => $"IndexBuffer Elements: {World.IndexBufferSize:N0} ({GetBytesReadable(World.IndexBufferSize * 4)})");
			_debugInfo.AddDebugLeft(() => $"Chunks: {World.ChunkCount}, {World.ChunkManager.RenderedChunks}, {World.ChunkManager.DrawCount}", TimeSpan.FromMilliseconds(500));
			_debugInfo.AddDebugLeft(() => $"Entities: {World.EntityManager.EntityCount}, {World.EntityManager.EntitiesRendered}, {World.EntityManager.DrawCount}", TimeSpan.FromMilliseconds(500));
			_debugInfo.AddDebugLeft(() => $"Particles: {Alex.ParticleManager.ParticleCount}", TimeSpan.FromMilliseconds(500));
			_debugInfo.AddDebugLeft(() =>
			{
				return $"Biome: {_currentBiome.Name} ({_currentBiomeId})";
			}, TimeSpan.FromMilliseconds(500));
			//_debugInfo.AddDebugLeft(() => { return $"Do DaylightCycle: {World.DoDaylightcycle}"; });

			_debugInfo.AddDebugLeft(
				() =>
				{
					return $"Exhaustion: {World.Player.HealthManager.Exhaustion:F1}/{World.Player.HealthManager.MaxExhaustion}";
				}, TimeSpan.FromMilliseconds(250));
			
			_debugInfo.AddDebugLeft(
				() =>
				{
					return $"Saturation: {World.Player.HealthManager.Saturation:F1}/{World.Player.HealthManager.MaxSaturation}";
				}, TimeSpan.FromMilliseconds(250));
			
			_debugInfo.AddDebugLeft(
				() =>
				{
					return $"Health: {World.Player.HealthManager.Health:F1}/{World.Player.HealthManager.MaxHealth}";
				}, TimeSpan.FromMilliseconds(250));
			
			_debugInfo.AddDebugLeft(
				() =>
				{
					return $"Gamemode: {World.Player.Gamemode}";
				}, TimeSpan.FromSeconds(5));
			
			_debugInfo.AddDebugRight(Alex.OperatingSystem);
			_debugInfo.AddDebugRight(Alex.Gpu);
			_debugInfo.AddDebugRight($"{Alex.DotnetRuntime}\n");
			_debugInfo.AddDebugRight(Alex.RenderingEngine);
			//_debugInfo.AddDebugRight(() => MemoryUsageDisplay);
			_debugInfo.AddDebugRight(() => $"RAM: {GetBytesReadable(_ramUsage, 2)}", TimeSpan.FromMilliseconds(1000));
			_debugInfo.AddDebugRight(() => $"GPU: {GetBytesReadable(GpuResourceManager.GetMemoryUsage, 2)}", TimeSpan.FromMilliseconds(1000));
			_debugInfo.AddDebugRight(() =>
			{
				return
					$"Threads: {(_threadsUsed):00}/{_maxThreads}";
			}, TimeSpan.FromMilliseconds(50));
			_debugInfo.AddDebugRight(() =>
			{
				var player = World?.Player;

				if (player == null)
					return "";
				
				if (player.HasRaytraceResult)
				{
					var raytracedBlock = player.RaytracedBlock;
					var adjacentBlock  = player.AdjacentRaytraceBlock;
					var adj             =  Vector3.Floor(adjacentBlock) - Vector3.Floor(raytracedBlock);
					adj.Normalize();

					var face = adj.GetBlockFace();

                    StringBuilder sb = new StringBuilder();
					sb.AppendLine($"Target: {raytracedBlock} Face: {face}");
					sb.AppendLine(
						$"Skylight: {World.GetSkyLight(raytracedBlock)} Face Skylight: {World.GetSkyLight(adjacentBlock)}");
					sb.AppendLine(
						$"Blocklight: {World.GetBlockLight(raytracedBlock)} Face Blocklight: {World.GetBlockLight(adjacentBlock)}");

					//sb.AppendLine($"Skylight scheduled: {World.IsScheduled((int) _raytracedBlock.X, (int) _raytracedBlock.Y, (int) _raytracedBlock.Z)}");
					
					foreach (var bs in World
						.GetBlockStates((int) raytracedBlock.X, (int) raytracedBlock.Y, (int) raytracedBlock.Z))
					{
						var blockstate = bs.State;
						if (blockstate != null && blockstate.Block.HasHitbox)
						{
							sb.AppendLine($"{blockstate.Name} (S: {bs.Storage})");
							/*if (blockstate.IsMultiPart)
							{
								sb.AppendLine($"MultiPart=true");
								sb.AppendLine();
								
								sb.AppendLine("Models:");

								foreach (var model in blockstate.AppliedModels)
								{
									sb.AppendLine(model);
								}
							}*/

							var dict = blockstate.ToDictionary();

							if (dict.Count > 0)
							{
								sb.AppendLine();
								sb.AppendLine("Blockstate:");

								foreach (var kv in dict)
								{
									sb.AppendLine($"{kv.Key}={kv.Value}");
								}
							}
						}
					}

					return sb.ToString();
				}
				else
				{
					return string.Empty;
				}
			}, TimeSpan.FromMilliseconds(500));
			
			_debugInfo.AddDebugRight(() =>
			{
				var player = World.Player;
				if (player == null || player.HitEntity == null) return string.Empty;

				var entity = player.HitEntity;
				return $"Hit entity: {entity.EntityId} / {entity.ToString()}\n{entity.NameTag}\n{ChatFormatting.Reset}Shown: {!entity.HideNameTag}\nNoAI: {entity.NoAi}\nGravity: {entity.IsAffectedByGravity}\nFlying: {entity.IsFlying}\nAllFlying: {entity.IsFlagAllFlying}\nOn Ground: {entity.KnownPosition.OnGround}\nHas Collisions: {entity.HasCollision}";
			}, TimeSpan.FromMilliseconds(500));
			/*
			_debugInfo.AddDebugRight(
				() =>
				{
					var item = World.Player?.Inventory?.MainHand;
					if (item == null) return string.Empty;

					var renderer = item.Renderer;
					return $"Hand:\n{item.Name}\n{renderer}";
				}, TimeSpan.FromMilliseconds(500));*/
		}

		private float AspectRatio { get; set; }

		private DateTime _previousMemUpdate = DateTime.UtcNow;

		protected override void OnUpdate(GameTime gameTime)
		{
			MiniMap.PlayerLocation = World.Player.KnownPosition;

			var args = new UpdateArgs() {Camera = World.Camera, GraphicsDevice = Graphics, GameTime = gameTime};

			_playingHud.CheckInput = Alex.GuiManager.ActiveDialog == null;

			//	if (Alex.IsActive)

			if (Math.Abs(AspectRatio - Graphics.Viewport.AspectRatio) > 0f)
			{
				World.Camera.UpdateAspectRatio(Graphics.Viewport.AspectRatio);
				AspectRatio = Graphics.Viewport.AspectRatio;
			}

			if (!_playingHud.Chat.Focused && Alex.GameStateManager.GetActiveState() is PlayingState)
			{
				World.Player.Controller.CheckMovementInput = Alex.IsActive && Alex.GuiManager.ActiveDialog == null;
				World.Player.Controller.CheckInput = Alex.IsActive;

				if (Alex.GuiManager.ActiveDialog == null)
				{
					CheckInput(gameTime);
				}
			}
			else
			{
				World.Player.Controller.CheckInput = false;
			}

			World.Update(args);

			var now = DateTime.UtcNow;

			if (now - _previousMemUpdate > TimeSpan.FromSeconds(5))
			{
				_previousMemUpdate = now;


				_ramUsage = Environment.WorkingSet;

				ThreadPool.GetMaxThreads(out int maxThreads, out _);
				ThreadPool.GetAvailableThreads(out int availableThreads, out _);
				_threadsUsed = maxThreads - availableThreads;

				_maxThreads = maxThreads;

				var pos     = World.Player.KnownPosition.GetCoordinates3D();
				var biomeId = World.GetBiome(pos.X, pos.Y, pos.Z);
				var biome   = BiomeUtils.GetBiomeById(biomeId);
				_currentBiomeId = biomeId;
				_currentBiome = biome;
			}

			var dir = World.Camera.Position - World.Camera.Target;
			dir.Normalize();
			dir = new Vector3(MathF.Round(dir.X), MathF.Round(dir.Y), MathF.Round(dir.Z));

			//dir.Normalize();
			Alex.AudioEngine.Update(gameTime, World.Camera.Position, dir);
			
			//Alex.ParticleManager.Update(gameTime);
			
			base.OnUpdate(gameTime);
		}

		//private Microsoft.Xna.Framework.BoundingBox RayTraceBoundingBox { get; set; }
		private bool _renderNetworking = true;

		private bool RenderNetworking
		{
			get
			{
				return _renderNetworking;
			}
			set
			{
				if (value != _renderNetworking)
				{
					_renderNetworking = value;

					if (value)
					{
						_networkDebugHud.Advanced = true;
						//Alex.GuiManager.AddScreen(_networkDebugHud);
					}
					else
					{
						_networkDebugHud.Advanced = false;
						//Alex.GuiManager.RemoveScreen(_networkDebugHud);
					}
				}
			}
		}
		private bool RenderDebug         { get; set; } = false;
		private bool RenderBoundingBoxes { get; set; } = false;

		private KeyboardState _oldKeyboardState;
		protected void CheckInput(GameTime gameTime) //TODO: Move this input out of the main update loop and use the new per-player based implementation by @TruDan
		{
			KeyboardState currentKeyboardState = Keyboard.GetState();
			if (currentKeyboardState != _oldKeyboardState)
			{
				if (KeyBinds.NetworkDebugging.All(x => currentKeyboardState.IsKeyDown(x)))
				{
					RenderNetworking = !RenderNetworking;
				}
				else if (KeyBinds.EntityBoundingBoxes.All(x => currentKeyboardState.IsKeyDown(x)))
				{
					RenderBoundingBoxes = !RenderBoundingBoxes;
				}
				else if (currentKeyboardState.IsKeyDown(KeyBinds.DebugInfo))
				{
					RenderDebug = !RenderDebug;
					if (!RenderDebug)
					{
						Alex.GuiManager.RemoveScreen(_debugInfo);
					}
					else
					{
						Alex.GuiManager.AddScreen(_debugInfo);
					}
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.Fog) && !_oldKeyboardState.IsKeyDown(KeyBinds.Fog))
				{
					World.ChunkManager.FogEnabled = !World.ChunkManager.FogEnabled;
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.ToggleWireframe))
				{
					World.ToggleWireFrame();
				}
			}

			_oldKeyboardState = currentKeyboardState;
		}

		protected void Draw2D(IRenderArgs args)
		{
			try
			{
				args.SpriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, blendState:BlendState.NonPremultiplied);

				if (World?.Player != null && World.Player.HasRaytraceResult)
				{
					var               player   = World?.Player;
					var               block    = player.SelBlock;
					//var               blockPos = player.RaytracedBlock;
					var boxes    = player.RaytraceBoundingBoxes;
					
					if (boxes != null && boxes.Length >0)
					{
						foreach (var boundingBox in boxes)
						{
							if (block.CanInteract || !World.Player.IsWorldImmutable)
							{
								Color color = Color.LightGray;

								if (World.Player.IsBreakingBlock)
								{
									var progress = World.Player.BlockBreakProgress;

									color = Color.Red * progress;

									var depth = args.GraphicsDevice.DepthStencilState;
									args.GraphicsDevice.DepthStencilState = DepthStencilState.None;

									args.SpriteBatch.RenderBoundingBox(
										boundingBox, World.Camera.ViewMatrix, World.Camera.ProjectionMatrix,
										color, true);

									args.GraphicsDevice.DepthStencilState = depth;
								}
								else
								{
									args.SpriteBatch.RenderBoundingBox(
										boundingBox, World.Camera.ViewMatrix, World.Camera.ProjectionMatrix,
										color);
								}
							}
						}
					}
				}

				if (RenderBoundingBoxes)
				{
					var hitEntity = World.Player?.HitEntity;

					var entities = World.Player?.EntitiesInRange;
					if (entities != null)
					{
						foreach (var entity in entities)
						{
							args.SpriteBatch.RenderBoundingBox(entity.GetBoundingBox(), World.Camera.ViewMatrix,
								World.Camera.ProjectionMatrix, entity == hitEntity ? Color.Red : Color.Yellow);
						}
					}

					if (World?.Player != null)
					{
						args.SpriteBatch.RenderBoundingBox(
							World.Player.GetBoundingBox(), World.Camera.ViewMatrix, World.Camera.ProjectionMatrix,
							Color.Red);

						var hit = World.Player.Movement.LastCollision;

						foreach (var bb in hit)
						{
							args.SpriteBatch.RenderBoundingBox(
								bb.Box, World.Camera.ViewMatrix, World.Camera.ProjectionMatrix, bb.Color, true);
						}
					}
				}
			}
			finally
			{
				args.SpriteBatch.End();
			}
			
			World?.Render2D(args);
			
			//Alex.ParticleManager.Draw(args.GameTime, World.Camera);
		}

		public static string GetCardinalDirection(PlayerLocation cam)
		{
			double rotation = (cam.HeadYaw) % 360;
			if (rotation < 0)
			{
				rotation += 360.0;
			}

			return GetDirection(rotation);
		}

		private static string GetDirection(double rotation)
		{
			if (0 <= rotation && rotation < 22.5)
			{
				return "South";
			}
			else if (22.5 <= rotation && rotation < 67.5)
			{
				return "South West";
			}
			else if (67.5 <= rotation && rotation < 112.5)
			{
				return "West";
			}
			else if (112.5 <= rotation && rotation < 157.5)
			{
				return "North West"; //
			}
			else if (157.5 <= rotation && rotation < 202.5)
			{
				return "North"; // 
			}
			else if (202.5 <= rotation && rotation < 247.5)
			{
				return "North East"; //
			}
			else if (247.5 <= rotation && rotation < 292.5)
			{
				return "East";
			}
			else if (292.5 <= rotation && rotation < 337.5)
			{
				return "South East";
			}
			else if (337.5 <= rotation && rotation < 360.0)
			{
				return "South";
			}
			else
			{
				return "N/A";
			}
		}

		public static string GetBytesReadable(long i, int decimals = 4)
		{
			// Get absolute value
			long absolute_i = (i < 0 ? -i : i);
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = (i >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = (i >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = (i >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = (i >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = (i >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = i;
			}
			else
			{
				return i.ToString("0 B"); // Byte
			}
			// Divide by 1024 to get fractional value
			readable = (readable / 1024);
			// Return formatted number with suffix
			return readable.ToString($"F{decimals}") + suffix;
		}

		protected override void OnDraw(IRenderArgs args)
		{
			args.Camera = World?.Camera;

			if (RenderMinimap)
			{
				MiniMap.Draw(args);
			}

			World?.Render(args);

			base.OnDraw(args);

			Draw2D(args);
		}

		protected override void OnUnload()
		{
			Alex.InGame = false;
			Alex.ParticleManager.Hide();
			ThreadPool.QueueUserWorkItem(
				o =>
				{
					NetworkProvider?.Close();
					NetworkProvider = null;
					
					World?.Dispose();
					World = null;
					
					WorldProvider?.Dispose();
					WorldProvider = null;

					_playingHud?.Unload();

					RichPresenceProvider.ClearPresence();
					GC.Collect();
				});

			
			//GetService<IEventDispatcher>().UnregisterEvents(_playingHud.Chat);
			//_playingHud.Chat = 
		}
	}
}
