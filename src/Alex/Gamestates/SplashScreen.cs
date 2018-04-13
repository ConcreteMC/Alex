using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;

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
			Gui = _screen = new SplashScreenGui(Alex)
			{
				BackgroundRepeatMode = TextureRepeatMode.Stretch
			};
		}

		protected override void OnLoad(RenderArgs args)
		{
			base.OnLoad(args);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			_screen.UpdateProgress((int)(100d * (gameTime.TotalGameTime.TotalMilliseconds / 2500)), "Loaddinnggg Stuffffssss....");
		}
	}

	public class SplashScreenGui : GuiScreen
	{
		private GuiContainer _container;

		private GuiTextElement _statusText, _percentageText;
		private GuiProgressBar _progressBar;

		public SplashScreenGui(Game game) : base(game)
		{
			AddChild( _container = new GuiContainer()
			{
				Width  = 300,
				Height = 30,

				Y = -50,
				
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment   = VerticalAlignment.Bottom,
			});

			_container.AddChild(_progressBar = new GuiProgressBar()
			         {
				         Width  = 300,
				         Height = 9,
				
				         HorizontalAlignment = HorizontalAlignment.Center,
				         VerticalAlignment   = VerticalAlignment.Bottom,
			         });

			_container.AddChild(_statusText = new GuiTextElement()
			{
				Text = "Loading...",
				Font = Alex.AltFont,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top
			});

			_container.AddChild(_percentageText = new GuiAutoUpdatingTextElement(() => $"{_progressBar.Percent:P}")
			{
				Font = Alex.AltFont,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top
			});
		}


		protected override void OnInit(IGuiRenderer renderer)
		{
			Background = renderer.GetTexture(GuiTextures.SplashBackground);
		}

		public void UpdateProgress(int value, string status)
		{
			_progressBar.Value = value;
			_statusText.Text = status;
		}
	}
}
