using System;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.GameStates.Gui.InGame;
using Alex.GameStates.Hud;
using Alex.Graphics.Models;
using Alex.Gui.Elements;
using Alex.Rendering.Camera;
using Alex.Rendering.UI;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.GameStates.Playing
{
	public class PlayingState : GameState
	{
		private SkyBox SkyRenderer { get; }
		public World World { get; }

		private FpsMonitor FpsCounter { get; set; }

		private WorldProvider WorldProvider { get; }
		public INetworkProvider NetworkProvider { get; }

		private readonly PlayingHud _playingHud;
		private readonly GuiDebugInfo _debugInfo;

		public PlayingState(Alex alex, GraphicsDevice graphics, WorldProvider worldProvider, INetworkProvider networkProvider) : base(alex)
		{
			NetworkProvider = networkProvider;

			World = new World(alex, graphics, new FirstPersonCamera(alex.GameSettings.RenderDistance, Vector3.Zero, Vector3.Zero), networkProvider);
			SkyRenderer = new SkyBox(alex, graphics, World);

			WorldProvider = worldProvider;
			if (worldProvider is SPWorldProvider)
			{
				World.FreezeWorldTime = true;
			}

			var chat = new ChatComponent();

			WorldProvider = worldProvider;
			WorldProvider.Init(World, chat, out var info, out var chatProvider);
			World.WorldInfo = info;
			chat.ChatProvider = chatProvider;

			_playingHud = new PlayingHud(Alex, World.Player, chat);
			_debugInfo = new GuiDebugInfo();
			FpsCounter = new FpsMonitor();
			InitDebugInfo();
		}

		protected override void OnLoad(IRenderArgs args)
		{
			World.SpawnPoint = WorldProvider.GetSpawnPoint();
			World.Camera.MoveTo(World.GetSpawnPoint(), Vector3.Zero);
			base.OnLoad(args);
		}

		protected override void OnShow()
		{
			Alex.IsMouseVisible = false;

			base.OnShow();
			Alex.GuiManager.AddScreen(_playingHud);
			Alex.GuiManager.AddScreen(_debugInfo);
		}

		protected override void OnHide()
		{
			Alex.GuiManager.RemoveScreen(_debugInfo);
			Alex.GuiManager.RemoveScreen(_playingHud);
			base.OnHide();
		}

		private void InitDebugInfo()
		{
			_debugInfo.AddDebugLeft(() =>
			{
				FpsCounter.Update();
				return $"Alex {Alex.Version} ({FpsCounter.Value:##} FPS, {World.ChunkUpdates}:{World.LowPriorityUpdates} chunk updates)";
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
				return $"Velocity: (X={pos.X:F2}, Y={pos.Y:F2}, Z={pos.Z:F2}) / SpeedFactor: {World.Player.Controller.LastSpeedFactor:F2}";
			});
			_debugInfo.AddDebugLeft(() => $"Vertices: {World.Vertices}");
			_debugInfo.AddDebugLeft(() => $"Chunks: {World.ChunkCount}, {World.ChunkManager.RenderedChunks}");
			_debugInfo.AddDebugLeft(() => $"Entities: {World.EntityManager.EntityCount}, {World.EntityManager.EntitiesRendered}");
			_debugInfo.AddDebugLeft(() =>
			{
				var pos = World.Player.KnownPosition.GetCoordinates3D();
				return $"Biome: {World.GetBiome(pos.X,pos.Y,pos.Z)}";
			});

			_debugInfo.AddDebugRight(() => Alex.DotnetRuntime);
			_debugInfo.AddDebugRight(() => MemoryUsageDisplay);
			_debugInfo.AddDebugRight(() => 
			{
				if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("Looking at: " + _raytracedBlock);
					sb.AppendLine($"{SelBlock}");

					if (SelBlock.BlockState != null)
					{
						if (SelBlock.BlockState is BlockState s && s.IsMultiPart)
						{
								sb.AppendLine($"MultiPart=true");
						}

						var dict = SelBlock.BlockState.ToDictionary();
						foreach (var kv in dict)
						{
							sb.AppendLine($"{kv.Key.Name}={kv.Value}");
						}
					}

					return sb.ToString();
				}
				else
				{
					return string.Empty;
				}
			});
		}

		private float AspectRatio { get; set; }
		private bool RenderWireframe { get; set; } = false;
		private string MemoryUsageDisplay { get; set; } = "";

		private TimeSpan _previousMemUpdate = TimeSpan.Zero;
		protected override void OnUpdate(GameTime gameTime)
		{
			var args = new UpdateArgs()
			{
				Camera = World.Camera,
				GraphicsDevice = Graphics,
				GameTime = gameTime
			};

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
					World.Player.Controller.CheckInput = Alex.IsActive;
					CheckInput(gameTime);
				}
				else
				{
					World.Player.Controller.CheckInput = false;
				}

				SkyRenderer.Update(args);
				World.Update(args, SkyRenderer);

				if (RenderDebug)
				{
					if (gameTime.TotalGameTime - _previousMemUpdate > TimeSpan.FromSeconds(5))
					{
						_previousMemUpdate = gameTime.TotalGameTime;
						//Alex.Process.Refresh();
						MemoryUsageDisplay = $"Allocated memory: {GetBytesReadable(Environment.WorkingSet)}";
					}
				}
			}
			base.OnUpdate(gameTime);
		}

		private Vector3 _raytracedBlock;
		protected void UpdateRayTracer(GraphicsDevice graphics, World world)
		{
			_raytracedBlock = RayTracer.Raytrace(graphics, world, World.Camera);
			if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
			{
				SelBlock = (Block)World.GetBlock(_raytracedBlock.X, _raytracedBlock.Y, _raytracedBlock.Z);
				RayTraceBoundingBox = SelBlock.GetBoundingBox(_raytracedBlock);
			}
			else
			{
				SelBlock = new Air();
			}
		}

		private Block SelBlock { get; set; } = new Air();
		private Microsoft.Xna.Framework.BoundingBox RayTraceBoundingBox { get; set; }
		private bool RenderDebug { get; set; } = true;

		private KeyboardState _oldKeyboardState;
		protected void CheckInput(GameTime gameTime) //TODO: Move this input out of the main update loop and use the new per-player based implementation by @TruDan
		{
			KeyboardState currentKeyboardState = Keyboard.GetState();
			if (currentKeyboardState != _oldKeyboardState)
			{
				if (currentKeyboardState.IsKeyDown(KeyBinds.DebugInfo))
				{
					RenderDebug = !RenderDebug;
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.ReBuildChunks))
				{
					World.RebuildChunks();
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.Fog) && !_oldKeyboardState.IsKeyDown(KeyBinds.Fog))
				{
					World.ChunkManager.OpaqueEffect.FogEnabled = !World.ChunkManager.OpaqueEffect.FogEnabled;
					World.ChunkManager.TransparentEffect.FogEnabled = !World.ChunkManager.TransparentEffect.FogEnabled;
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.ChangeCamera))
				{
					if (World.Camera is FirstPersonCamera)
					{
						World.Camera = new ThirdPersonCamera(Alex.GameSettings.RenderDistance, World.Player.KnownPosition, Vector3.Zero);
					}
					else
					{
						World.Camera = new FirstPersonCamera(Alex.GameSettings.RenderDistance, World.Player.KnownPosition, Vector3.Zero);
					}
				}
			}

			_oldKeyboardState = currentKeyboardState;
		}

		protected void Draw2D(IRenderArgs args)
		{
			try
			{
				args.SpriteBatch.Begin();

				if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
				{
					args.SpriteBatch.RenderBoundingBox(
						RayTraceBoundingBox,
						World.Camera.ViewMatrix, World.Camera.ProjectionMatrix, Color.LightGray);
				}

				World.Render2D(args);
			}
			finally
			{
				args.SpriteBatch.End();
			}
		}

		public static string GetCardinalDirection(PlayerLocation cam)
		{
			double rotation = (360 - cam.HeadYaw) % 360;
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
				return "North";
			}
			else if (22.5 <= rotation && rotation < 67.5)
			{
				return "North East";
			}
			else if (67.5 <= rotation && rotation < 112.5)
			{
				return "East";
			}
			else if (112.5 <= rotation && rotation < 157.5)
			{
				return "South East";
			}
			else if (157.5 <= rotation && rotation < 202.5)
			{
				return "South";
			}
			else if (202.5 <= rotation && rotation < 247.5)
			{
				return "South West";
			}
			else if (247.5 <= rotation && rotation < 292.5)
			{
				return "West";
			}
			else if (292.5 <= rotation && rotation < 337.5)
			{
				return "North West";
			}
			else if (337.5 <= rotation && rotation < 360.0)
			{
				return "North";
			}
			else
			{
				return "N/A";
			}
		}

		private static string GetBytesReadable(long i)
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
			return readable.ToString("0.### ") + suffix;
		}

		protected override void OnDraw(IRenderArgs args)
		{
			args.Camera = World.Camera;

			FpsCounter.Update();
			
			SkyRenderer.Draw(args);

			World.Render(args);

			base.OnDraw(args);

			Draw2D(args);
		}

		protected override void OnUnload()
		{
			World.Destroy();
			WorldProvider.Dispose();
		}
	}
}
