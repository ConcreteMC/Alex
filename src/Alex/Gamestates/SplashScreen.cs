using Alex.Rendering.UI;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class SplashScreen : GameState
	{
		private Texture2D Background { get; }
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

		protected override void OnDraw2D(RenderArgs args)
		{
			args.GraphicsDevice.Clear(Color.White);
			args.SpriteBatch.Begin(SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			args.SpriteBatch.Draw(Background, Vector2.Zero, Color.White);
			args.SpriteBatch.End();
			base.OnDraw2D(args);
		}
	}
}
