using System.Drawing;
using System.Windows.Forms;
using Alex.Properties;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class PlayingState : Gamestate
	{
		private Texture2D CrosshairTexture { get; set; }
		public override void Init(RenderArgs args)
		{
			CrosshairTexture = ResManager.ImageToTexture2D(Resources.crosshair);
			base.Init(args);
		}

		public override void Stop()
		{
			base.Stop();
		}

		public override void Render2D(RenderArgs args)
		{
			FpsCounter.Render(args.GraphicsDevice);
			args.SpriteBatch.Begin();
			args.SpriteBatch.Draw(CrosshairTexture, new Vector2(CenterScreen.X - CrosshairTexture.Width / 2, CenterScreen.Y - CrosshairTexture.Height / 2));
			args.SpriteBatch.End();
			base.Render2D(args);
		}

		public override void Render3D(RenderArgs args)
		{
			Alex.Instance.World.Render();
			base.Render3D(args);
		}

		public override void OnUpdate(GameTime gameTime)
		{
			Alex.Instance.UpdateCamera(gameTime);
			if (Alex.Instance.IsActive)
			{
				Alex.Instance.HandleInput();
			}

			base.OnUpdate(gameTime);
		}
	}
}
