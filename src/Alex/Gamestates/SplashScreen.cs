using Alex.Gamestates.Gui;
using Alex.Graphics.Gui;
using Alex.Graphics.Gui.Elements;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.UI.Common;
using Alex.Rendering.UI;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class SplashScreen : GameState
	{

		private Vector2 StatusTextPosition { get; set; }

		private string StatusText { get; set; }
		private Vector2 StatusTextSize { get; set; }

		private SplashScreenGui _screen;

		public SplashScreen(Alex alex) : base(alex)
		{
		}

		protected override void OnLoad(RenderArgs args)
		{
			GuiManager.AddScreen(_screen = new SplashScreenGui(Alex));
			base.OnLoad(args);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			_screen.UpdateProgress((int)(100d * (gameTime.TotalGameTime.TotalMilliseconds / 2500)));
		}
	}

	public class SplashScreenGui : GuiScreen
	{
		private GuiProgressBar _progressBar;
		public SplashScreenGui(Game game) : base(game)
		{

		}


		protected override void OnInit(IGuiRenderer renderer)
		{
			Background = renderer.GetTexture(GuiTextures.SplashBackground);
			_progressBar = new GuiProgressBar()
			{
							Width  = 300,
							Height = 9,

							Y = -50,
				
							HorizontalAlignment = HorizontalAlignment.Center,
							VerticalAlignment   = VerticalAlignment.Bottom,
			};
			AddChild(_progressBar);
		}

		public void UpdateProgress(int value)
		{
			_progressBar.Value = value;
		}
	}
}
