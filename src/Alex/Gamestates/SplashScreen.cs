using Alex.API.Gui;
using Alex.API.Gui.Elements;

using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates
{
	public class SplashScreen : GuiGameStateBase, IProgressReceiver
	{
		private readonly Container _progressBarContainer;

		private readonly ProgressBar _progressBar;
		private readonly TextElement _textDisplay;
		private readonly TextElement _subTextDisplay;
        private readonly TextElement _percentageDisplay;
		
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
			Background.TextureResource = AlexGuiTextures.SplashBackground;
			Background.RepeatMode = TextureRepeatMode.ScaleToFit;

			AddChild(_progressBarContainer = new Container()
			{
				Width  = 300,
				Height = 35,
				Margin = new Thickness(12),
				
				Anchor = Alignment.BottomCenter,
			});

			_progressBarContainer.AddChild(_textDisplay = new TextElement()
			{
				Text      = Text,
				TextColor = (Color) TextColor.Black,
				
				Anchor    = Alignment.TopLeft,
				HasShadow = false
			});

			_progressBarContainer.AddChild(_percentageDisplay = new TextElement()
			{
				Text      = Text,
				TextColor = (Color) TextColor.Black,
				
				Anchor    = Alignment.TopRight,
				HasShadow = false
			});

			_progressBarContainer.AddChild(_progressBar = new ProgressBar()
			{
				Width  = 300,
				Height = 9,
				
				Anchor = Alignment.MiddleCenter,
			});

			_progressBarContainer.AddChild(_subTextDisplay = new TextElement()
			{
				Text = Text,
				TextColor = (Color) TextColor.Black,

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
