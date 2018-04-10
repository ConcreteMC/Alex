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

		protected override void OnLoad(RenderArgs args)
		{
			//Controls.Add("logo", new Logo()
			//{
			//	DrawMotd = false,
			//	Center = true
			//});
		}

		private void UpdateStatusText(float percent, string statusText)
		{
			StatusText = statusText;
			StatusTextSize = Alex.Font.MeasureString(statusText);

			var width = Viewport.TitleSafeArea.Width * 0.75;
			var height = 10;

			ProgressBarBounds = new Rectangle((int)(Viewport.TitleSafeArea.Left + width / 2f), (int)(Viewport.TitleSafeArea.Bottom - 50), (int)width, height);
			ProgressBarFillBounds = new Rectangle((int)(Viewport.TitleSafeArea.Left + width / 2f), (int)(Viewport.TitleSafeArea.Bottom - 50), (int)(width*percent), height);
		}

		protected override void OnDraw2D(RenderArgs args)
		{
			args.GraphicsDevice.Clear(Color.White);
			args.SpriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			
			// Background
			args.SpriteBatch.Draw(Background, Vector2.Zero, Color.White);

			// Status Text

			// Progress Bar
			//args.SpriteBatch.DrawRectangle(ProgressBarBounds, Color.LightSlateGray);
			//args.SpriteBatch.DrawRectangle(ProgressBarBounds, Color.FromNonPremultiplied(0xFF, 0x50, 0x3F, 0xFF));

			args.SpriteBatch.End();
			base.OnDraw2D(args);
		}
	}
}
