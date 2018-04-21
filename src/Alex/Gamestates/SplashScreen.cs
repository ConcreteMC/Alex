using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;

namespace Alex.GameStates
{
	public class SplashScreen : GameState, IProgressReceiver
	{
		private SplashScreenGui _screen;

		public SplashScreen(Alex alex) : base(alex)
		{
			Gui = _screen = new SplashScreenGui(Alex)
			{
				BackgroundRepeatMode = TextureRepeatMode.Stretch,
				Anchor = Alignment.TopLeft,
			};
		}

		protected override void OnLoad(RenderArgs args)
		{
			base.OnLoad(args);
		}

		public void UpdateProgress(int percentage, string statusMessage)
		{
			_screen.UpdateProgress(percentage);
			_screen.Text = statusMessage;
		}
	}

	public class SplashScreenGui : GuiScreen
	{
		private GuiContainer _progressBarContainer;

		private GuiProgressBar _progressBar;
		private GuiTextElement _textDisplay;
		private GuiTextElement _percentageDisplay;

		public string Text
		{
			get { return _textDisplay?.Text ?? string.Empty; }
			set
			{
				_textDisplay.Text = value;
			}
		}

		private Alex Alex { get; }
		public SplashScreenGui(Alex game) : base(game)
		{
			Alex = game;
			
			AddChild(_progressBarContainer = new GuiContainer()
			{
				Width  = 300,
				Height = 25,
				Margin = new Thickness(12),
				
				Anchor = Alignment.BottomCenter,
			});

			_progressBarContainer.AddChild(_textDisplay = new GuiTextElement()
			{
				Text      = Text,
				TextColor = TextColor.Black,
				
				Anchor    = Alignment.TopLeft,
				HasShadow = false
			});

			_progressBarContainer.AddChild(_percentageDisplay = new GuiTextElement()
			{
				Text      = Text,
				TextColor = TextColor.Black,
				
				Anchor    = Alignment.TopRight,
				HasShadow = false
			});

			_progressBarContainer.AddChild(_progressBar = new GuiProgressBar()
			{
				Width  = 300,
				Height = 9,
				
				Anchor = Alignment.BottomCenter,
			});
		}

		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);

			Background = renderer.GetTexture(GuiTextures.SplashBackground);
			//AddChild(_progressBarContainer = new GuiStackContainer()
			//{
			//	Width  = 300,
			//	Height = 10,

			//	Y = -50,

			//	HorizontalAlignment = HorizontalAlignment.Center,
			//	VerticalAlignment   = VerticalAlignment.MaxY,

			//	VerticalContentAlignment = VerticalAlignment.Center,
			//	HorizontalContentAlignment = HorizontalAlignment.FillParent
			//});
		}

		public void UpdateProgress(int value)
		{
			_progressBar.Value = value;
			_percentageDisplay.Text = $"{value}%";
			//_percentageDisplay.Y = _percentageDisplay.Height;
		}
	}
}
