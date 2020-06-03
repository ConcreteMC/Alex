using System;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.API;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Blocks.Minecraft;
using Alex.Gamestates.InGame.Hud;
using Alex.Graphics.Camera;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Items;
using Alex.Net;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace Alex.Gamestates.InGame
{
	public class PlayingState : GameState
	{
		public World World { get; }

        private WorldProvider WorldProvider { get; }
		public NetworkProvider NetworkProvider { get; }

		private readonly PlayingHud _playingHud;
		private readonly GuiDebugInfo _debugInfo;
		private readonly NetworkDebugHud _networkDebugHud;
		
		private GuiMiniMap MiniMap { get; }
		private bool RenderMinimap { get; set; } = false;
		public PlayingState(Alex alex, GraphicsDevice graphics, WorldProvider worldProvider, NetworkProvider networkProvider) : base(alex)
		{
			NetworkProvider = networkProvider;

			World = new World(alex.Services, graphics, Options, new FirstPersonCamera(Options.VideoOptions.RenderDistance, Vector3.Zero, Vector3.Zero), networkProvider);
			World.Player.IsFirstPersonMode = true;
			
			WorldProvider = worldProvider;
			if (worldProvider is SPWorldProvider)
			{
				World.DoDaylightcycle = false;
				//World.Player.SetInventory(new BedrockInventory(46));
				if (ItemFactory.TryGetItem("minecraft:diamond_sword", out var sword))
				{
					World.Player.Inventory.MainHand = sword;
					World.Player.Inventory[World.Player.Inventory.SelectedSlot] = sword;
				}
			}
			
			var title = new TitleComponent();

			WorldProvider = worldProvider;
			WorldProvider.Init(World, out var info);
			World.WorldInfo = info;
			
			WorldProvider.TitleComponent = title;

			_playingHud = new PlayingHud(Alex, World.Player, title);
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
			
			if (RenderNetworking)
				Alex.GuiManager.AddScreen(_networkDebugHud);
			
			base.OnShow();
			Alex.GuiManager.AddScreen(_playingHud);

			if (RenderDebug)
				Alex.GuiManager.AddScreen(_debugInfo);
		}

		protected override void OnHide()
		{
			Alex.GuiManager.RemoveScreen(_debugInfo);
			Alex.GuiManager.RemoveScreen(_playingHud);
			Alex.GuiManager.RemoveScreen(_networkDebugHud);
			
			base.OnHide();
		}

		private long _ramUsage = 0;
		private long _threadsUsed, _maxThreads, _complPortUsed, _maxComplPorts;
		private Biome _currentBiome = BiomeUtils.GetBiomeById(0);
		private int _currentBiomeId = 0;
		private DateTime _lastNetworkInfo = DateTime.UtcNow;
		private void InitDebugInfo()
		{
			_debugInfo.AddDebugLeft(() =>
			{
				//FpsCounter.Update();
				//World.ChunkManager.GetPendingLightingUpdates(out int lowLight, out int midLight, out int highLight);

				double avg = 0;
				if (World.ChunkManager.TotalChunkUpdates > 0)
				{
					avg = (World.ChunkManager.ChunkUpdateTime / World.ChunkManager.TotalChunkUpdates).TotalMilliseconds;
				}

				return
					$"Alex {Alex.Version} ({Alex.FpsMonitor.Value:##} FPS, Chunk Updates: {World.EnqueuedChunkUpdates} queued, {World.ConcurrentChunkUpdates} active, Avg: {avg:F2}ms, Max: {World.ChunkManager.MaxUpdateTime.TotalMilliseconds:F2}ms, Min: {World.ChunkManager.MinUpdateTIme.TotalMilliseconds:F2})" /*, H: {highLight} M: {midLight} L: {lowLight} lighting updates)"*/
					;
			});
			_debugInfo.AddDebugLeft(() =>
			{
				var pos = World.Player.KnownPosition;
				var blockPos = pos.GetCoordinates3D();
				return $"RenderPosition: (X={pos.X:F2}, Y={pos.Y:F2}, Z={pos.Z:F2}) / Block: ({blockPos.X:D}, {blockPos.Y:D}, {blockPos.Z:D})";
			});
			_debugInfo.AddDebugLeft(() =>
			{
				var pos = World.Player.KnownPosition;
				return  $"Facing: {GetCardinalDirection(pos)} (HeadYaw={pos.HeadYaw:F2}, Yaw={pos.Yaw:F2}, Pitch={pos.Pitch:F2})";
			});
			_debugInfo.AddDebugLeft(() =>
			{
				var pos = World.Player.Velocity;
				return $"Velocity: (X={pos.X:F2}, Y={pos.Y:F2}, Z={pos.Z:F2}) / Target Speed: {World.Player.Controller.LastSpeedFactor:F2} M/s";
			});
			_debugInfo.AddDebugLeft(() => $"Vertices: {World.Vertices:N0} ({GetBytesReadable((long)(World.Vertices * BlockShaderVertex.VertexDeclaration.VertexStride))})");
		//	_debugInfo.AddDebugLeft(() => $"IndexBuffer Elements: {World.IndexBufferSize:N0} ({GetBytesReadable(World.IndexBufferSize * 4)})");
			_debugInfo.AddDebugLeft(() => $"Chunks: {World.ChunkCount}, {World.ChunkManager.RenderedChunks}");
			_debugInfo.AddDebugLeft(() => $"Entities: {World.EntityManager.EntityCount}, {World.EntityManager.EntitiesRendered}");
			_debugInfo.AddDebugLeft(() =>
			{
				return $"Biome: {_currentBiome.Name} ({_currentBiomeId})";
			});
			_debugInfo.AddDebugLeft(() => { return $"Do DaylightCycle: {World.DoDaylightcycle}"; });

			_debugInfo.AddDebugRight(() => Alex.OperatingSystem);
			_debugInfo.AddDebugRight(() => Alex.Gpu);
			_debugInfo.AddDebugRight(() => $"{Alex.DotnetRuntime}\n");
			//_debugInfo.AddDebugRight(() => MemoryUsageDisplay);
			_debugInfo.AddDebugRight(() => $"RAM: {GetBytesReadable(_ramUsage, 2)}");
			_debugInfo.AddDebugRight(() => $"GPU: {GetBytesReadable(GpuResourceManager.GetMemoryUsage, 2)}");
			_debugInfo.AddDebugRight(() =>
			{
				return
					$"Threads: {(_threadsUsed):00}/{_maxThreads}";
			});
			_debugInfo.AddDebugRight(() => 
			{
				if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
				{
					var adj =  Vector3.Floor(_adjacentBlock) - Vector3.Floor(_raytracedBlock);
					adj.Normalize();

					var face = adj.GetBlockFace();

                    StringBuilder sb = new StringBuilder();
					sb.AppendLine($"Target: {_raytracedBlock} Face: {face}");
					sb.AppendLine(
						$"Skylight: {World.GetSkyLight(_raytracedBlock)} Face Skylight: {World.GetSkyLight(_adjacentBlock)}");
					sb.AppendLine(
						$"Blocklight: {World.GetBlockLight(_raytracedBlock)} Face Blocklight: {World.GetBlockLight(_adjacentBlock)}");

					//sb.AppendLine($"Skylight scheduled: {World.IsScheduled((int) _raytracedBlock.X, (int) _raytracedBlock.Y, (int) _raytracedBlock.Z)}");
					
					foreach (var bs in World
						.GetBlockStates((int) _raytracedBlock.X, (int) _raytracedBlock.Y, (int) _raytracedBlock.Z))
					{
						var blockstate = bs.State;
						if (blockstate != null && blockstate.Block.Renderable)
						{
							sb.AppendLine($"{blockstate.Name} (S: {bs.Storage})");
							if (blockstate.IsMultiPart)
							{
								sb.AppendLine($"MultiPart=true");
								sb.AppendLine();
								
								sb.AppendLine("Models:");

								foreach (var model in blockstate.AppliedModels)
								{
									sb.AppendLine(model);
								}
							}

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
			});
			
			_debugInfo.AddDebugRight(() =>
			{
				var player = World.Player;
				if (player == null || player.HitEntity == null) return string.Empty;

				var entity = player.HitEntity;
				return $"Hit entity: {entity.EntityId} / {entity.ToString()}";
			});
		}

		private float AspectRatio { get; set; }
		private string MemoryUsageDisplay { get; set; } = "";

		private DateTime _previousMemUpdate = DateTime.UtcNow;
		protected override void OnUpdate(GameTime gameTime)
		{
			MiniMap.PlayerLocation = World.Player.KnownPosition;
			
			var args = new UpdateArgs()
			{
				Camera = World.Camera,
				GraphicsDevice = Graphics,
				GameTime = gameTime
			};

			_playingHud.CheckInput = Alex.GuiManager.ActiveDialog == null;
			
		//	if (Alex.IsActive)
			{
				var newAspectRatio = Graphics.Viewport.AspectRatio;
				if (AspectRatio != newAspectRatio)
				{
					World.Camera.UpdateAspectRatio(newAspectRatio);
					AspectRatio = newAspectRatio;
				}

				UpdateRayTracer(Alex.GraphicsDevice, World);

				if (!_playingHud.Chat.Focused)
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

				if (AlwaysDay){
					World.SetTime(1200);
				}
				
				World.Update(args);

				var now = DateTime.UtcNow;
				if (now - _previousMemUpdate > TimeSpan.FromSeconds(5))
				{
					_previousMemUpdate = now;

					//Task.Run(() =>
					{
						_ramUsage = Environment.WorkingSet;

						ThreadPool.GetMaxThreads(out int maxThreads, out int maxCompletionPorts);
						ThreadPool.GetAvailableThreads(out int availableThreads, out int availableComplPorts);
						_threadsUsed = maxThreads - availableThreads;
						_complPortUsed = maxCompletionPorts - availableComplPorts;

						_maxThreads = maxThreads;
						_maxComplPorts = maxCompletionPorts;
						
						var pos = World.Player.KnownPosition.GetCoordinates3D();
						var biomeId = World.GetBiome(pos.X, pos.Y, pos.Z);
						var biome = BiomeUtils.GetBiomeById(biomeId);
						_currentBiomeId = biomeId;
						_currentBiome = biome;
					}//);
				}
			}
			base.OnUpdate(gameTime);
		}

	    private Air _air = new Air();
		private Vector3 _raytracedBlock;
	    private Vector3 _adjacentBlock;
		protected void UpdateRayTracer(GraphicsDevice graphics, World world)
		{
		    var camPos = world.Camera.Position;
		    var lookVector = world.Camera.Direction;

		    for (float x = 0.5f; x < 8f; x += 0.1f)
		    {
		        Vector3 targetPoint = camPos + (lookVector * x);
		        var block = world.GetBlock(targetPoint) as Block;

		        if (block != null && block.HasHitbox)
		        {
		            var bbox = block.GetBoundingBox(Vector3.Floor(targetPoint));
		            if (bbox.Contains(targetPoint) == ContainmentType.Contains)
		            {
		                _raytracedBlock = Vector3.Floor(targetPoint);
                        SelBlock = block;
		                RayTraceBoundingBox = bbox;

			            world.Player.Raytraced = targetPoint;
			            world.Player.HasRaytraceResult = true;

                        if (SetPlayerAdjacentSelectedBlock(world, x, camPos, lookVector, out Vector3 rawAdjacent))
                        {
	                        _adjacentBlock = Vector3.Floor(rawAdjacent);

				            world.Player.AdjacentRaytrace = rawAdjacent;
                            world.Player.HasAdjacentRaytrace = true;
                        }
			            else
			            {
				            world.Player.HasAdjacentRaytrace = false;
			            }

                        return;
		            }
		        }
		    }

		    SelBlock = _air;
		    _raytracedBlock.Y = 999;
		    world.Player.HasRaytraceResult = false;
		    world.Player.HasAdjacentRaytrace = false;
		}

	    private bool SetPlayerAdjacentSelectedBlock(World world, float xStart, Vector3 camPos, Vector3 lookVector, out Vector3 rawAdjacent)
	    {
	        for (float x = xStart; x > 0.7f; x -= 0.1f)
	        {
	            Vector3 targetPoint = camPos + (lookVector * x);
	            var block = world.GetBlock(targetPoint) as Block;

	            if (block != null && (!block.Solid))
	            {
		            rawAdjacent = targetPoint;
	                return true;
	            }
            }

			rawAdjacent = new Vector3(0, 0, 0);
	        return false;
	    }


	    private Block SelBlock { get; set; } = new Air();
		private Microsoft.Xna.Framework.BoundingBox RayTraceBoundingBox { get; set; }
		private bool RenderNetworking { get; set; } = false;
		private bool RenderDebug { get; set; } = false;
		private bool RenderBoundingBoxes { get; set; } = false;
		private bool AlwaysDay { get; set; } = false;
		
		private KeyboardState _oldKeyboardState;
		protected void CheckInput(GameTime gameTime) //TODO: Move this input out of the main update loop and use the new per-player based implementation by @TruDan
		{
			KeyboardState currentKeyboardState = Keyboard.GetState();
			if (currentKeyboardState != _oldKeyboardState)
			{
				/*if (currentKeyboardState.IsKeyUp(Keys.Add) && _oldKeyboardState.IsKeyDown(Keys.Add))
				{
					if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
					{
						Entity.NametagScale += 0.1f;
					}
					else
					{
						Entity.NametagScale += 0.25f;
					}
				}
				else if (currentKeyboardState.IsKeyUp(Keys.Subtract) && _oldKeyboardState.IsKeyDown(Keys.Subtract))
				{
					if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
					{
						Entity.NametagScale -= 0.1f;
					}
					else
					{
						Entity.NametagScale -= 0.25f;
					}
				}*/

				if (KeyBinds.NetworkDebugging.All(x => currentKeyboardState.IsKeyDown(x)))
				{
					RenderNetworking = !RenderNetworking;
					if (!RenderNetworking)
					{
						Alex.GuiManager.RemoveScreen(_networkDebugHud);
					}
					else
					{
						Alex.GuiManager.AddScreen(_networkDebugHud);
					}
				}
				else if (KeyBinds.EntityBoundingBoxes.All(x => currentKeyboardState.IsKeyDown(x)))
				{
					RenderBoundingBoxes = !RenderBoundingBoxes;
				}
				else if (KeyBinds.AlwaysDay.All(x => currentKeyboardState.IsKeyDown(x)))
				{
					if (!AlwaysDay)
					{
						World.SetTime(1200);
					}
					AlwaysDay = !AlwaysDay;
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

				if (currentKeyboardState.IsKeyDown(KeyBinds.ReBuildChunks))
				{
					World.RebuildChunks();
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.Fog) && !_oldKeyboardState.IsKeyDown(KeyBinds.Fog))
				{
					World.ChunkManager.FogEnabled = !World.ChunkManager.FogEnabled;
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.ChangeCamera))
				{
					if (World.Camera is FirstPersonCamera)
					{
						World.Camera = new ThirdPersonCamera(Options.VideoOptions.RenderDistance, World.Camera.Position, World.Camera.Rotation)
						{
							FOV = World.Camera.FOV
						};
						
						World.Camera.UpdateAspectRatio(Graphics.Viewport.AspectRatio);

						if (World?.Player?.ItemRenderer != null)
						{
							World.Player.ItemRenderer.DisplayPosition = World.Player.IsLeftyHandy ?
								DisplayPosition.ThirdPersonLeftHand : DisplayPosition.ThirdPersonRightHand;
						}

						if (World?.Player != null)
						{
							World.Player.IsFirstPersonMode = false;
						}
					}
					else
					{
						World.Camera = new FirstPersonCamera(Options.VideoOptions.RenderDistance, World.Camera.Position, World.Camera.Rotation)
						{
							FOV = World.Camera.FOV
						};
						
						World.Camera.UpdateAspectRatio(Graphics.Viewport.AspectRatio);

						if (World?.Player != null)
						{
							World.Player.IsFirstPersonMode = true;

							if (World.Player.ItemRenderer != null)
							{
								World.Player.ItemRenderer.DisplayPosition = World.Player.IsLeftyHandy ?
									DisplayPosition.FirstPersonLeftHand : DisplayPosition.FirstPersonRightHand;
							}
						}
					}
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

				if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
				{
					if (SelBlock.CanInteract || !World.Player.IsWorldImmutable)
					{
						Color color = Color.LightGray;
						if (World.Player.IsBreakingBlock)
						{
							var progress = World.Player.BlockBreakProgress;
							
							color = Color.Red * progress;
							
							var depth = args.GraphicsDevice.DepthStencilState;
							args.GraphicsDevice.DepthStencilState = DepthStencilState.None;
							
							args.SpriteBatch.RenderBoundingBox(
								RayTraceBoundingBox,
								World.Camera.ViewMatrix, World.Camera.ProjectionMatrix, color, true);

							args.GraphicsDevice.DepthStencilState = depth;
						}
						else
						{
							args.SpriteBatch.RenderBoundingBox(
								RayTraceBoundingBox,
								World.Camera.ViewMatrix, World.Camera.ProjectionMatrix, color);
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
						args.SpriteBatch.RenderBoundingBox(World.Player.GetBoundingBox(), World.Camera.ViewMatrix,
							World.Camera.ProjectionMatrix, Color.Red);

					if (World.PhysicsEngine.LastKnownHit != null)
					{
						foreach (var bb in World.PhysicsEngine.LastKnownHit)
						{
							args.SpriteBatch.RenderBoundingBox(bb, World.Camera.ViewMatrix,
								World.Camera.ProjectionMatrix, Color.YellowGreen);
						}
					}
				}
			}
			finally
			{
				args.SpriteBatch.End();
			}
			
			World.Render2D(args);
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
			args.Camera = World.Camera;

			if (RenderMinimap)
			{
				MiniMap.Draw(args);
			}

			World.Render(args);

			base.OnDraw(args);

			Draw2D(args);
		}

		protected override void OnUnload()
		{
			Alex.InGame = false;
			
			World.Destroy();
			WorldProvider.Dispose();
			NetworkProvider.Close();

			_playingHud.Unload();
			//GetService<IEventDispatcher>().UnregisterEvents(_playingHud.Chat);
			//_playingHud.Chat = 
		}
	}
}
