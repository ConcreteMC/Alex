using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.World;
using Alex.Blocks;
using Alex.Properties;
using Alex.Rendering;
using Alex.Rendering.Camera;
using Alex.Utils;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MiNET.Utils;

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

		public PlayingState(Alex alex, GraphicsDevice graphics, WorldProvider worldProvider) : base(graphics)
		{
			Alex = alex;
			Camera = new FirstPersonCamera(alex.GameSettings.RenderDistance, Vector3.Zero, Vector3.Zero);

			World = new World(alex, graphics, Camera, worldProvider);
			Camera.MoveTo(World.GetSpawnPoint(), Vector3.Zero);

			CamComponent = new CameraComponent(Camera, Graphics, World, alex.GameSettings);
		}

		public override void Init(RenderArgs args)
		{
			ChatMessages = new List<string>();
			Alex.OnCharacterInput += OnCharacterInput;

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

				CamComponent.Update(gameTime, !RenderChatInput);

				UpdateRayTracer(Alex.GraphicsDevice, World);

				CheckInput(gameTime);

				World.Update();
			}
			base.OnUpdate(gameTime);
		}

		private Vector3 _raytracedBlock;
		protected void UpdateRayTracer(GraphicsDevice graphics, World world)
		{
			_raytracedBlock = RayTracer.Raytrace(graphics, world, Camera);
			if (_raytracedBlock.Y > 0 && _raytracedBlock.Y < 256)
			{
				SelBlock = (Block)World.GetBlock(_raytracedBlock.X, _raytracedBlock.Y, _raytracedBlock.Z);
				RayTraceBoundingBox = SelBlock.GetBoundingBox(_raytracedBlock);
			}
		}

		private void OnCharacterInput(object sender, char c)
		{
			if (RenderChatInput)
			{
#if FNA
				if (c == (char)8) //BackSpace
				{
					BackSpace();
					return;
				}
				if (c == (char) 13)
				{
					SubmitMessage();
					return;
				}
#endif
				_input += c;
			}
		}

		private void BackSpace()
		{
			if (_input.Length > 0) _input = _input.Remove(_input.Length - 1, 1);
		}

		private void SubmitMessage()
		{
			//Submit message
			if (_input.Length > 0)
			{
				if (Alex.IsMultiplayer)
				{
					//Client.SendChat(_input);
				}
				else
				{
					ChatMessages.Add("<Alex> " + _input);
				}
			}
			_input = string.Empty;
			RenderChatInput = false;
		}

		//private string[]
		private Block SelBlock { get; set; } = new Air();
		private Microsoft.Xna.Framework.BoundingBox RayTraceBoundingBox { get; set; }

		private List<string> ChatMessages { get; set; }
		private string _input = "";
		private bool RenderDebug { get; set; } = true;
		private bool RenderChatInput { get; set; } = false;
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
					if (RenderChatInput)
					{
						RenderChatInput = false;
					}
					else
					{
						Alex.GamestateManager.SetActiveState(new InGameMenuState(Alex, this, currentKeyboardState));
					}
				}

				if (RenderChatInput) //Handle Input
				{
#if MONOGAME
					if (currentKeyboardState.IsKeyDown(Keys.Back))
					{
						BackSpace();
					}

					if (currentKeyboardState.IsKeyDown(Keys.Enter))
					{
						SubmitMessage();
					}
#endif
				}
				else
				{
					if (currentKeyboardState.IsKeyDown(KeyBinds.Chat))
					{
						RenderChatInput = !RenderChatInput;
						_input = string.Empty;
					}

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

#if MONOGAME
				args.SpriteBatch.Draw(CrosshairTexture,
					new Vector2(CenterScreen.X - CrosshairTexture.Width / 2f, CenterScreen.Y - CrosshairTexture.Height / 2f));
#endif
#if FNA
				args.SpriteBatch.Draw(CrosshairTexture,
					new Vector2(CenterScreen.X - CrosshairTexture.Width/2f, CenterScreen.Y - CrosshairTexture.Height/2f),
					Color.White);
#endif

				if (RenderChatInput)
				{
					var heightCalc = Alex.Font.MeasureString("!");
					string chatInput = _input.StripIllegalCharacters();
					if (chatInput.Length > 0)
					{
						heightCalc = Alex.Font.MeasureString(chatInput);
					}

					int extra = 0;
					if (heightCalc.X > args.GraphicsDevice.Viewport.Width / 2f)
					{
						extra = (int)(heightCalc.X - args.GraphicsDevice.Viewport.Width / 2f);
					}

					args.SpriteBatch.FillRectangle(
						new Rectangle(0, (int)(args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 5)),
							(args.GraphicsDevice.Viewport.Width / 2) + extra, (int)heightCalc.Y),
						new Color(Color.Black, 64));

					args.SpriteBatch.DrawString(Alex.Font, chatInput,
						new Vector2(5, (int)(args.GraphicsDevice.Viewport.Height - (heightCalc.Y + 5))), Color.White);
				}

			/*	var count = 2;
				foreach (var msg in ChatMessages.TakeLast(5).Reverse())
				{
					var amsg = msg.StripColors();
					amsg = amsg.StripIllegalCharacters();
					var heightCalc = Alex.Font.MeasureString(amsg);

					int extra = 0;
					if (heightCalc.X > args.GraphicsDevice.Viewport.Width / 2f)
					{
						extra = (int)(heightCalc.X - args.GraphicsDevice.Viewport.Width / 2f);
					}

					args.SpriteBatch.FillRectangle(
						new Rectangle(0, (int)(args.GraphicsDevice.Viewport.Height - ((heightCalc.Y * count) + 10)),
							(args.GraphicsDevice.Viewport.Width / 2) + extra, (int)heightCalc.Y),
						new Color(Color.Black, 64));
					args.SpriteBatch.DrawString(Alex.Font, amsg,
						new Vector2(5, (int)(args.GraphicsDevice.Viewport.Height - ((heightCalc.Y * count) + 10))), Color.White);
					count++;
				}*/

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
			base.Render2D(args);
		}

		public override void Render3D(RenderArgs args)
		{
			FpsCounter.Update();

			World.Render();

			base.Render3D(args);
		}

		public override void Stop()
		{
			World.Destroy();
		}
	}
}
