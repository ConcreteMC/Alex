using System;
using Alex.Gamestates.Gui;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Rendering;
using Alex.Rendering.UI;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates
{
	public class SplashScreen : GameState, IProgressReceiver
	{
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

		public void UpdateProgress(int percentage, string statusMessage)
		{
			_screen.UpdateProgress(percentage);
			_screen.Text = statusMessage;
		}
	}

	public class SplashScreenGui : GuiScreen
	{
		private GuiContainer _container;

		private GuiTextElement _statusText, _percentageText;
		private GuiProgressBar _progressBar;
		private GuiTextElement _textDisplay;
		private GuiTextElement _percentageDisplay;

		public string Text
		{
			get { return _textDisplay?.Text ?? string.Empty; }
			set
			{
				_textDisplay.Text = value;
				_textDisplay.Y = -(_textDisplay.Height);
			}
		}

		private Alex Alex { get; }
		public SplashScreenGui(Alex game) : base(game)
		{
			Alex = game;
		}

		protected override void OnInit(IGuiRenderer renderer)
		{
			Background = renderer.GetTexture(GuiTextures.SplashBackground);
			_progressBar = new GuiProgressBar()
			{
				Width = 300,
				Height = 9,

				Y = -50,

				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Bottom,
			};
			AddChild(_progressBar);

			_textDisplay  = new GuiTextElement()
			{
				Text = Text,
				Font = Alex.Font,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Top,
				Scale = 0.5f
			};
			_progressBar.AddChild(_textDisplay);

			_percentageDisplay = new GuiTextElement()
			{
				Text = Text,
				Font = Alex.Font,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Bottom,
				Scale = 0.50f
			};
			_progressBar.AddChild(_percentageDisplay);
		}

		public void UpdateProgress(int value, string status)
		{
			_progressBar.Value = value;
			_percentageDisplay.Text = $"{value}%";
			_percentageDisplay.Y = _percentageDisplay.Height;
		}
	}
}
