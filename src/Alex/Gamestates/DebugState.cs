using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.Blocks;
using Alex.Gamestates.Playing;
using Alex.Graphics.Items;
using Alex.Properties;
using Alex.Rendering;
using Alex.Rendering.Camera;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET.Utils;
using Dirt = MiNET.Blocks.Dirt;

namespace Alex.Gamestates
{
	class DebugState : Gamestate
	{
		private Alex Alex { get; }
		private World World { get; }
		private FirstPersonCamera Camera;
		private CameraComponent CamComponent { get; }
		private int renderDistance;

		private FpsMonitor FpsCounter { get; set; }
		private Texture2D CrosshairTexture { get; set; }
		private IWorldGenerator Generator { get; set; }

		private List<ChunkCoordinates> LoadedChunks { get; } = new List<ChunkCoordinates>();
		public DebugState(Alex alex, GraphicsDevice graphics) : base(graphics)
		{
			Alex = alex;
			Camera = new FirstPersonCamera(alex.GameSettings.RenderDistance, Vector3.Zero, Vector3.Zero);
			World = new World(graphics, Camera);

			renderDistance = alex.GameSettings.RenderDistance;

			CamComponent = new CameraComponent(Camera, Graphics, World, alex.GameSettings);
		}

		private ChunkCoordinates PreviousChunkCoordinates { get; set; } = new ChunkCoordinates(int.MaxValue, int.MaxValue);
		public override void Init(RenderArgs args)
		{
			FpsCounter = new FpsMonitor();
			CrosshairTexture = ResManager.ImageToTexture2D(Resources.crosshair);

			World.ResetChunks();
			Generator = new AnvilWorldProvider("E:\\MinecraftWorlds\\Vanilla")
			//Generator = new AnvilWorldProvider("E:\\MinecraftWorlds\\TestWorld")
			{
				MissingChunkProvider = new VoidWorldGenerator()
			};
			Generator.Initialize();

			int totalChunks = 64;
			int t = totalChunks / 8;
			new Thread(() =>
			{
				while (true)
				{
					ChunkCoordinates currentCoordinates =
						new ChunkCoordinates(new PlayerLocation(Camera.Position.X, Camera.Position.Y, Camera.Position.Z));

					if (PreviousChunkCoordinates.DistanceTo(currentCoordinates) >= 1)
					{
						PreviousChunkCoordinates = currentCoordinates;

						var oldChunks = LoadedChunks.ToArray();

						List<ChunkCoordinates> newChunkCoordinates = new List<ChunkCoordinates>();
						for (int x = -t; x < t; x++)
						{
							for (int z = -t; z < t; z++)
							{
								var cc = currentCoordinates + new ChunkCoordinates(x, z);
								if (!LoadedChunks.Contains(cc))
								{
									var chunk =
										Generator.GenerateChunkColumn(cc);

									if (chunk == null) continue;
									
									World.ChunkManager.AddChunk(chunk,
										new Vector3(cc.X, 0, cc.Z), true);

									LoadedChunks.Add(cc);
									newChunkCoordinates.Add(cc);
								}
							}
						}

						foreach (var chunk in oldChunks)
						{
							if (!newChunkCoordinates.Contains(chunk) && currentCoordinates.DistanceTo(chunk) > t + 1)
							{
								World.ChunkManager.RemoveChunk(new Vector3(chunk.X, 0, chunk.Z));
								LoadedChunks.Remove(chunk);
							}
						}
					}

					Thread.Sleep(500);
				}
			})
			{
				IsBackground = true
			}.Start();			
		    
            Camera.MoveTo(Generator.GetSpawnPoint(), Vector3.Zero);
            base.Init(args);
		}

		public override void OnUpdate(GameTime gameTime)
		{
			if (Alex.IsActive)
			{
				UpdateRayTracer(Alex.GraphicsDevice, World);

				CheckInput(gameTime);
			}
			base.OnUpdate(gameTime);
		}

		private Vector3 _raytracedBlock;
		protected void UpdateRayTracer(GraphicsDevice graphics, World world)
		{
			_raytracedBlock = RayTracer.Raytrace(graphics, world, Camera);
		}

		private bool RenderDebug { get; set; } = true;
		private KeyboardState _oldKeyboardState;
		private MouseState _oldMouseState;
		protected void CheckInput(GameTime gameTime)
		{
			CamComponent.Update(gameTime, Alex.IsActive);

			MouseState currentMouseState = Mouse.GetState();
			if (currentMouseState != _oldMouseState)
			{
			/*	if (currentMouseState.LeftButton == ButtonState.Pressed)
				{
					if (_selectedBlock.Y > 0 && _selectedBlock.Y < 256)
					{
						World.SetBlock(_selectedBlock.X, _selectedBlock.Y, _selectedBlock.Z, new Air());
					}
				}

				if (currentMouseState.RightButton == ButtonState.Pressed)
				{
					if (_selectedBlock.Y > 0 && _selectedBlock.Y < 256)
					{
						World.SetBlock(_selectedBlock.X, _selectedBlock.Y + 1, _selectedBlock.Z, new Stone());
					}
				} */
			}
			_oldMouseState = currentMouseState;

			KeyboardState currentKeyboardState = Keyboard.GetState();
			if (currentKeyboardState != _oldKeyboardState)
			{
				if (currentKeyboardState.IsKeyDown(KeyBinds.DebugInfo))
				{
					RenderDebug = !RenderDebug;
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.ToggleFreeCam))
				{
					CamComponent.IsFreeCam = !CamComponent.IsFreeCam;
				}

				if (currentKeyboardState.IsKeyDown(KeyBinds.ReBuildChunks))
				{
					World.RebuildChunks();
				}
			}
			_oldKeyboardState = currentKeyboardState;
		}

		public override void Render2D(RenderArgs args)
		{
			try
			{
				args.SpriteBatch.Begin();

#if MONOGAME
				args.SpriteBatch.Draw(CrosshairTexture,
					new Vector2(CenterScreen.X - CrosshairTexture.Width / 2f, CenterScreen.Y - CrosshairTexture.Height / 2f));
#endif
#if FNA
				args.SpriteBatch.Draw(CrosshairTexture,
					new Vector2(CenterScreen.X - CrosshairTexture.Width/2f, CenterScreen.Y - CrosshairTexture.Height/2f),
					Color.White);
#endif
				Block selBlock = new Air();

				if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
				{
					selBlock = World.GetBlock(_raytracedBlock.X, _raytracedBlock.Y, _raytracedBlock.Z);
					var boundingBox = new Microsoft.Xna.Framework.BoundingBox(_raytracedBlock + selBlock.BlockModel.Offset,
						_raytracedBlock + selBlock.BlockModel.Offset + selBlock.BlockModel.Size);

					args.SpriteBatch.RenderBoundingBox(
						boundingBox,
						Camera.ViewMatrix, Camera.ProjectionMatrix, Color.LightGray);
				}

				if (RenderDebug)
				{
					var fpsString = string.Format("Alex {0} ({1} FPS, {2} chunk updates)", Alex.Version,
						Math.Round(FpsCounter.Value), World.ChunkUpdates);
					var meisured = Alex.Font.MeasureString(fpsString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, 0, (int)meisured.X, (int)meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font,
						fpsString, new Vector2(0, 0),
						Color.White);

					var y = (int)meisured.Y;
					var positionString = "Position: " + Camera.Position;
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

					y += (int)meisured.Y;

					positionString = "Looking at: " + _raytracedBlock;
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

					y += (int)meisured.Y;

					positionString = string.Format("Block: {0} ID: {1}:{2}", selBlock, selBlock.BlockId, selBlock.Metadata);
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

					y += (int)meisured.Y;

					positionString = "Vertices: " + World.Vertices;
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

					y += (int)meisured.Y;

					positionString = "Chunks: " + World.ChunkCount;
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);
				}
			}
			finally
			{
				args.SpriteBatch.End();
			}
			base.Render2D(args);
		}

		public override void Render3D(RenderArgs args)
		{
			Camera.UpdateAspectRatio(args.GraphicsDevice.Viewport.AspectRatio);
			FpsCounter.Update();
			World.Render();

			base.Render3D(args);
		}
	}
}
