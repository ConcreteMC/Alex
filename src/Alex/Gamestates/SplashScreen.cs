using Alex.Rendering.UI;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class SplashScreen : GameState
	{
		private Texture2D Background { get; }


		private Vector2 StatusTextPosition { get; set; }

		private string StatusText { get; set; }
		private Vector2 StatusTextSize { get; set; }

		private Rectangle ProgressBarBounds { get; set; }
		private Rectangle ProgressBarFillBounds { get; set; }
		private float ProgressPercent { get; set; }

		public SplashScreen(Alex alex) : base(alex)
		{
			Background = TextureUtils.ImageToTexture2D(alex.GraphicsDevice, Resources.Splash);
		}

		protected override void OnDraw2D(RenderArgs args)
		{
			args.GraphicsDevice.Clear(Color.White);
			args.SpriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			args.SpriteBatch.Draw(Background, CenterScreen - new Vector2(Background.Width / 2f, Background.Height / 2f), Color.White);
			args.SpriteBatch.End();
			base.OnDraw2D(args);
		}
	}
}
