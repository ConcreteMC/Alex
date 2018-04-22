using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.GameStates.Gui.Common;
using Microsoft.Xna.Framework;

namespace Alex.GameStates
{
	public class SplashScreen : GuiGameStateBase, IProgressReceiver
	{
		private readonly GuiContainer _progressBarContainer;

		private readonly GuiProgressBar _progressBar;
		private readonly GuiTextElement _textDisplay;
		private readonly GuiTextElement _percentageDisplay;
		
		public string Text
		{
			get { return _textDisplay?.Text ?? string.Empty; }
			set
			{
				_textDisplay.Text = value;
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
		

		public void UpdateProgress(int percentage, string statusMessage)
		{
			_progressBar.Value      = percentage;
			_percentageDisplay.Text = $"{percentage}%";
			Text = statusMessage;
		}
	}
}
