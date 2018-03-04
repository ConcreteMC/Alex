using System;
using Alex.API.World;
using Alex.Blocks;
using Alex.Graphics.Overlays;
using Alex.Rendering.Camera;
using Alex.Rendering.UI;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.Gamestates.Playing
{
	public class PlayingState : Gamestate
	{
		private Alex Alex { get; }
		private World World { get; }
		private FirstPersonCamera Camera;
		private CameraComponent CamComponent { get; }

		private FpsMonitor FpsCounter { get; set; }
		private Texture2D CrosshairTexture { get; set; }

		private ChatComponent Chat { get; }
		//private ThreadSafeList<IOverlay> ActiveOverlays { get; }
 	//	private WaterOverlay WaterOverlay { get; }
		public PlayingState(Alex alex, GraphicsDevice graphics, WorldProvider worldProvider) : base(graphics)
		{
			Alex = alex;
			Chat = new ChatComponent();

			Camera = new FirstPersonCamera(alex.GameSettings.RenderDistance, Vector3.Zero, Vector3.Zero);

			World = new World(alex, graphics, Camera, worldProvider);
			Camera.MoveTo(World.GetSpawnPoint(), Vector3.Zero);

			CamComponent = new CameraComponent(Camera, Graphics, World, alex.GameSettings);

			//ActiveOverlays = new ThreadSafeList<IOverlay>();
			//WaterOverlay = new WaterOverlay();
		}

		public override void Init(RenderArgs args)
		{
			//WaterOverlay.Load(args.GraphicsDevice, Alex.Resources);

			Controls.Add("chatComponent", Chat);

			FpsCounter = new FpsMonitor();
			CrosshairTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.crosshair);

			Camera.MoveTo(World.GetSpawnPoint(), Vector3.Zero);
            base.Init(args);
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

				CamComponent.Update(gameTime, !Chat.RenderChatInput);

				UpdateRayTracer(Alex.GraphicsDevice, World);

				CheckInput(gameTime);

				World.Update(gameTime);

				var headBlock = World.GetBlock(Camera.Position);
				if (headBlock.BlockId == 8 || headBlock.BlockId == 9)
				{
					if (!_renderWaterOverlay)
					{
						_renderWaterOverlay = true;
					}
					//if (!ActiveOverlays.Contains(WaterOverlay))
				//	{
					//	ActiveOverlays.TryAdd(WaterOverlay);
					//}
				}else if (_renderWaterOverlay)
				{
					_renderWaterOverlay = false;
				}
			}
			base.OnUpdate(gameTime);
		}

		private bool _renderWaterOverlay = false;

		private Vector3 _raytracedBlock;
		protected void UpdateRayTracer(GraphicsDevice graphics, World world)
		{
			_raytracedBlock = RayTracer.Raytrace(graphics, world, Camera);
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
		private MouseState _oldMouseState;
		protected void CheckInput(GameTime gameTime)
		{
			MouseState currentMouseState = Mouse.GetState();
			if (currentMouseState != _oldMouseState)
			{
				if (currentMouseState.LeftButton == ButtonState.Pressed)
				{
					if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
					{
						//World.SetBlock(_selectedBlock.X, _selectedBlock.Y, _selectedBlock.Z, new Air());
					}
				}

				if (currentMouseState.RightButton == ButtonState.Pressed)
				{
					if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
					{
					//	World.SetBlock(_selectedBlock.X, _selectedBlock.Y + 1, _selectedBlock.Z, new Stone());
					}
				}
			}
			_oldMouseState = currentMouseState;

			KeyboardState currentKeyboardState = Keyboard.GetState();
			if (currentKeyboardState != _oldKeyboardState)
			{
				if (currentKeyboardState.IsKeyDown(KeyBinds.Menu))
				{
					if (Chat.RenderChatInput)
					{
						Chat.Dismiss();			
					}
					else
					{
						Alex.GamestateManager.SetActiveState(new InGameMenuState(Alex, this, currentKeyboardState));
					}
				}

				if (!Chat.RenderChatInput)
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
			}
			_oldKeyboardState = currentKeyboardState;
		}

		public override void Render2D(RenderArgs args)
		{
			try
			{
				args.SpriteBatch.Begin();


				if (_renderWaterOverlay)
				{
					//Start draw background
					var retval = new Microsoft.Xna.Framework.Rectangle(
						args.SpriteBatch.GraphicsDevice.Viewport.X,
						args.SpriteBatch.GraphicsDevice.Viewport.Y,
						args.SpriteBatch.GraphicsDevice.Viewport.Width,
						args.SpriteBatch.GraphicsDevice.Viewport.Height);
					args.SpriteBatch.FillRectangle(retval, new Color(Color.DarkBlue, 0.5f));
					//End draw backgroun
				}

				args.SpriteBatch.Draw(CrosshairTexture,
					new Vector2(CenterScreen.X - CrosshairTexture.Width / 2f, CenterScreen.Y - CrosshairTexture.Height / 2f));

				if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
				{
					args.SpriteBatch.RenderBoundingBox(
						RayTraceBoundingBox,
						Camera.ViewMatrix, Camera.ProjectionMatrix, Color.LightGray);
				}
				var fpsString = string.Format("Alex {0} ({1} FPS, {2} chunk updates)", Alex.Version,
					Math.Round(FpsCounter.Value), World.ChunkUpdates);
				var meisured = Alex.Font.MeasureString(fpsString);

				args.SpriteBatch.FillRectangle(new Rectangle(0, 0, (int)meisured.X, (int)meisured.Y),
					new Color(Color.Black, 64));
				args.SpriteBatch.DrawString(Alex.Font,
					fpsString, new Vector2(0, 0),
					Color.White);

				if (RenderDebug)
				{
					var y = (int)meisured.Y;
					var positionString = "Position: " + Camera.Position;
					meisured = Alex.Font.MeasureString(positionString);

					args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

					y += (int)meisured.Y;
					string facing = GetCardinalDirection(this.Camera);

					positionString = string.Format("Facing: {0}", facing);
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

					positionString = string.Format("Block: {0} ID: {1}:{2}", SelBlock, SelBlock.BlockId, SelBlock.Metadata);
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

		//	ActiveOverlays.ForEach(x => x.Render(args));

			base.Render2D(args);
		}

		public static string GetCardinalDirection(FirstPersonCamera cam)
		{
			double rotation = (cam.Yaw) % 360;
			if (rotation < 0)
			{
				rotation += 360.0;
			}
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


		public override void Render3D(RenderArgs args)
		{
			FpsCounter.Update();

			World.Render(args);

			base.Render3D(args);
		}

		public override void Stop()
		{
			World.Destroy();
		}
	}
}
