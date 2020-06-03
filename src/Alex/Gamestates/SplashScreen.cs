using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates
{
	public class SplashScreen : GuiGameStateBase, IProgressReceiver
	{
		private readonly GuiContainer _progressBarContainer;

		private readonly GuiProgressBar _progressBar;
		private readonly GuiTextElement _textDisplay;
		private readonly GuiTextElement _subTextDisplay;
        private readonly GuiTextElement _percentageDisplay;
		
		public string Text
		{
			get { return _textDisplay?.Text ?? string.Empty; }
			set
			{
				_textDisplay.Text = value;
			}
		}

		public string SubText
		{
			get { return _subTextDisplay?.Text ?? string.Empty; }
			set
			{
				_subTextDisplay.Text = value;
			}
		}

        public SplashScreen()
		{
			Background = Color.White;
			Background.TextureResource = GuiTextures.SplashBackground;
			Background.RepeatMode = TextureRepeatMode.ScaleToFit;

			AddChild(_progressBarContainer = new GuiContainer()
			{
				Width  = 300,
				Height = 35,
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
				
				Anchor = Alignment.MiddleCenter,
			});

			_progressBarContainer.AddChild(_subTextDisplay = new GuiTextElement()
			{
				Text = Text,
				TextColor = TextColor.Black,

				Anchor = Alignment.BottomLeft,
				HasShadow = false
			});
        }
		

		public void UpdateProgress(int percentage, string statusMessage)
		{
			_progressBar.Value      = percentage;
			_percentageDisplay.Text = $"{percentage}%";
			Text = statusMessage;
			SubText = "";

		}

		public void UpdateProgress(int percentage, string statusMessage, string sub)
		{
			_progressBar.Value = percentage;
			_percentageDisplay.Text = $"{percentage}%";

			if (statusMessage != null)
			{
				Text = statusMessage;
			}

			SubText = sub;
		}
	}
}
