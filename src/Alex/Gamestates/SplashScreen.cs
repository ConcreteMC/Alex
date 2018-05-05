using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Alex.GameStates.Gui.Common;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Elements.Layout;

namespace Alex.GameStates
{
	public class SplashScreen : GuiGameStateBase, IProgressReceiver
	{
		private readonly GuiContainer _progressBarContainer;

		private readonly GuiProgressBar _progressBar;
		private readonly GuiMCTextElement _textDisplay;
		private readonly GuiMCTextElement _percentageDisplay;
		
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
				
				Anchor = Anchor.BottomCenter,
			});

			_progressBarContainer.AddChild(_textDisplay = new GuiMCTextElement()
			{
				Text      = Text,
				TextColor = TextColor.Black,
				
				Anchor    = Anchor.TopLeft
			});

			_progressBarContainer.AddChild(_percentageDisplay = new GuiMCTextElement()
			{
				Text      = Text,
				TextColor = TextColor.Black,
				
				Anchor    = Anchor.TopRight
			});

			_progressBarContainer.AddChild(_progressBar = new GuiProgressBar()
			{
				Width  = 300,
				Height = 9,
				
				Anchor = Anchor.BottomCenter,
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
