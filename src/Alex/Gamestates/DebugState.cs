using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.World;
using Alex.Blocks;
using Alex.Gamestates.Playing;
using Alex.Properties;
using Alex.Rendering;
using Alex.Rendering.Camera;
using Alex.Utils;
using Alex.Worlds;
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

		private FpsMonitor FpsCounter { get; set; }
		private Texture2D CrosshairTexture { get; set; }
		//private IWorldGenerator Generator { get; set; }

		private WorldProvider WorldProvider { get; set; }
		public DebugState(Alex alex, GraphicsDevice graphics) : base(graphics)
		{
			Alex = alex;
			Camera = new FirstPersonCamera(alex.GameSettings.RenderDistance, Vector3.Zero, Vector3.Zero);
			World = new World(alex, graphics, Camera);

			CamComponent = new CameraComponent(Camera, Graphics, World, alex.GameSettings);
		}

		public override void Init(RenderArgs args)
		{
			FpsCounter = new FpsMonitor();
			CrosshairTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.crosshair);

			World.ResetChunks();
			IWorldGenerator Generator;
			//Generator = new AnvilWorldProvider("E:\\SlicNic24\'s Resource Pack Test Map")
		//	Generator = new AnvilWorldProvider("E:\\MinecraftWorlds\\Vanilla")
			Generator = new AnvilWorldProvider("E:\\MinecraftWorlds\\KingsLanding1")
			//Generator = new AnvilWorldProvider("C:\\Users\\kennyvv\\Desktop\\Debug\\world")
			//Generator = new AnvilWorldProvider("E:\\Kenny\\AppData\\Roaming\\.minecraft\\saves\\DebugWorld")
			{
				MissingChunkProvider = new VoidWorldGenerator()
			};
			Generator.Initialize();

			WorldProvider = new SPWorldProvider(Alex, Camera, OnChunkReceived, Unload, Generator);

			Camera.MoveTo(Generator.GetSpawnPoint(), Vector3.Zero);
            base.Init(args);
		}

		private void Unload(int x, int z)
		{
			World.ChunkManager.RemoveChunk(new ChunkCoordinates(x,z));
		}

		private void OnChunkReceived(IChunkColumn chunkColumn, int x, int z)
		{
			World.ChunkManager.AddChunk(chunkColumn, new ChunkCoordinates(x, z));
		}

		private float AspectRatio { get; set; }
		public override void OnUpdate(GameTime gameTime)
		{
			if (Alex.IsActive)
			{
				var newAspectRatio = Graphics.Viewport.AspectRatio;
				if (AspectRatio != newAspectRatio)
				{
					Camera.UpdateAspectRatio(newAspectRatio);
					AspectRatio = newAspectRatio;
				}

				CamComponent.Update(gameTime, Alex.IsActive);

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
					selBlock = (Block)World.GetBlock(_raytracedBlock.X, _raytracedBlock.Y, _raytracedBlock.Z);
					var boundingBox = selBlock.GetBoundingBox(_raytracedBlock);

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

					positionString = "Chunks: " + World.ChunkCount + ", " + World.ChunkManager.RenderedChunks;
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
			FpsCounter.Update();

			World.Render();

			base.Render3D(args);
		}
	}
}
