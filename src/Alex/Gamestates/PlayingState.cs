using System;
using Alex.Blocks;
using Alex.Properties;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex.Gamestates
{
	public class PlayingState : Gamestate
	{
		private FrameCounter FpsCounter { get; set; }
		private Texture2D CrosshairTexture { get; set; }
		private bool RenderDebug { get; set; } = false;

		public override void Init(RenderArgs args)
		{
			OldKeyboardState = Keyboard.GetState();
			FpsCounter = new FrameCounter();
			CrosshairTexture = ResManager.ImageToTexture2D(Resources.crosshair);
			SelectedBlock = Vector3.Zero;
			base.Init(args);
		}

		public override void Stop()
		{
			base.Stop();
		}

		public override void Render2D(RenderArgs args)
		{
			FpsCounter.Update((float) args.GameTime.ElapsedGameTime.TotalSeconds);

			args.SpriteBatch.Begin();
			args.SpriteBatch.Draw(CrosshairTexture,
				new Vector2(CenterScreen.X - CrosshairTexture.Width/2f, CenterScreen.Y - CrosshairTexture.Height/2f));

			if (RenderDebug)
			{
				var fpsString = string.Format("Alex {0} ({1} FPS)", Alex.Version, Math.Round(FpsCounter.AverageFramesPerSecond));
				var meisured = Alex.Font.MeasureString(fpsString);

				args.SpriteBatch.FillRectangle(new Rectangle(0, 0, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
				args.SpriteBatch.DrawString(Alex.Font,
					fpsString, new Vector2(0, 0),
					Color.White);

				var y = (int) meisured.Y;
				var positionString = "Position: " + Game.MainCamera.Position;
				meisured = Alex.Font.MeasureString(positionString);

				args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
				args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0,y), Color.White);

				y += (int)meisured.Y;

				positionString = "Looking at: " + SelectedBlock;
				meisured = Alex.Font.MeasureString(positionString);

				args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
				args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

                y += (int)meisured.Y;

                positionString = "Vertices: " + Alex.Instance.World.Vertices;
                meisured = Alex.Font.MeasureString(positionString);

                args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
                args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);

                y += (int)meisured.Y;

                positionString = "Chunks: " + Alex.Instance.World.ChunkCount;
                meisured = Alex.Font.MeasureString(positionString);

                args.SpriteBatch.FillRectangle(new Rectangle(0, y, (int)meisured.X, (int)meisured.Y), new Color(Color.Black, 64));
                args.SpriteBatch.DrawString(Alex.Font, positionString, new Vector2(0, y), Color.White);
            }
			args.SpriteBatch.End();

			if (SelectedBlock.Y > 0 && SelectedBlock.Y < 256)
			{
				var selBlock = Alex.Instance.World.GetBlock(SelectedBlock.X, SelectedBlock.Y, SelectedBlock.Z);
				var boundingBox = new BoundingBox(SelectedBlock + selBlock.BlockModel.Offset,
					SelectedBlock + selBlock.BlockModel.Offset + selBlock.BlockModel.Size);

				args.SpriteBatch.RenderBoundingBox(
					boundingBox,
					Game.MainCamera.ViewMatrix, Game.MainCamera.ProjectionMatrix, Color.LightGray);
			}

			base.Render2D(args);
		}

		public override void Render3D(RenderArgs args)
		{
			Alex.Instance.World.Render();
			base.Render3D(args);
		}

		private Vector3 SelectedBlock;
		private KeyboardState OldKeyboardState;
	    private MouseState OldMouseState;
		public override void OnUpdate(GameTime gameTime)
		{
			SelectedBlock = RayTracer.Raytrace();
			Alex.Instance.UpdateCamera(gameTime);
			if (Alex.Instance.IsActive)
			{
				Alex.Instance.HandleInput();

			    MouseState currentMouseState = Mouse.GetState();
			    if (currentMouseState != OldMouseState)
			    {
			        if (currentMouseState.LeftButton == ButtonState.Pressed)
			        {
			            if (SelectedBlock.Y > 0 && SelectedBlock.Y < 256)
			            {
			                Alex.Instance.World.SetBlock(SelectedBlock.X, SelectedBlock.Y, SelectedBlock.Z, new Air());
			            }
			        }

			        if (currentMouseState.RightButton == ButtonState.Pressed)
			        {
			            if (SelectedBlock.Y > 0 && SelectedBlock.Y < 256)
			            {
			                Alex.Instance.World.SetBlock(SelectedBlock.X, SelectedBlock.Y + 1, SelectedBlock.Z, new Stone());
			            }
			        }
			    }
			    OldMouseState = currentMouseState;

				KeyboardState currentKeyboardState = Keyboard.GetState();
				if (currentKeyboardState != OldKeyboardState)
				{
					if (currentKeyboardState.IsKeyDown(KeyBinds.Menu))
					{
						
					}

					if (currentKeyboardState.IsKeyDown(KeyBinds.DebugInfo))
					{
						RenderDebug = !RenderDebug;
					}
				}
				OldKeyboardState = currentKeyboardState;
			}

			base.OnUpdate(gameTime);
		}
	}
}
