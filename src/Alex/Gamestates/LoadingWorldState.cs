using System.Drawing;
using System.IO;
using Alex.API.World;
using Alex.Graphics.Gui;
using Alex.Graphics.Gui.Elements;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.Textures;
using Alex.Graphics.UI;
using Alex.Graphics.UI.Common;
using Alex.Utils;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Gamestates
{
    public class LoadingWorldState : GameState
    {
		private Texture2D Background { get; }
		private LoadingWorldGui Screen { get; set; }
		public LoadingWorldState(Alex alex, Texture2D background) : base(alex)
	    {
		    Background = background;
	    }

	    public void UpdateProgress(LoadingState state, int percentage)
	    {
		    switch (state)
		    {
				case LoadingState.ConnectingToServer:
					Screen.Text = "Connecting to server...";
					break;
				case LoadingState.LoadingChunks:
					Screen.Text = $"Loading chunks...";
					break;
				case LoadingState.GeneratingVertices:
					Screen.Text = $"Building world...";
					break;
				case LoadingState.Spawning:
					Screen.Text = $"Getting ready...";
					break;
		    }

			Screen.UpdateProgress(percentage);
	    }

	    protected override void OnLoad(RenderArgs args)
	    {
		    base.OnLoad(args);

		    Screen = new LoadingWorldGui(Alex);
		    Screen.Background = Background;
		    GuiManager.AddScreen(Screen);
		}

	    protected override void OnShow()
	    {
		    base.OnShow();
	    }

	    private class LoadingWorldGui : GuiScreen
	    {
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
		    public LoadingWorldGui(Alex game) : base(game)
		    {
			    Alex = game;
		    }

		    protected override void OnInit(IGuiRenderer renderer)
		    {
			    Texture2D logoTexture;
			    using (MemoryStream ms = new MemoryStream(Resources.logo))
			    {
				    logoTexture = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, new Bitmap(ms));
			    }

			    var logo = new GuiTextureElement()
			    {
				    Texture = logoTexture,
				    Height = 50,
					Width = 150,
					//X = -75,
					Y = -25,
				    HorizontalAlignment = HorizontalAlignment.Center,
				    VerticalAlignment = VerticalAlignment.Center,
					RepeatMode = TextureRepeatMode.Stretch
			    };

				AddChild(logo);

			    _progressBar = new GuiProgressBar()
			    {
				    Width = 300,
				    Height = 9,

				    Y = -50,

				    HorizontalAlignment = HorizontalAlignment.Center,
				    VerticalAlignment = VerticalAlignment.Bottom,
			    };
			    AddChild(_progressBar);

			    _textDisplay = new GuiTextElement()
			    {
				    Text = Text,
				    Font = Alex.Font,
				    HorizontalAlignment = HorizontalAlignment.Center,
				    VerticalAlignment = VerticalAlignment.Top,
				    Scale = 0.5f,
					Color = Color.White
			    };
			    _progressBar.AddChild(_textDisplay);

			    _percentageDisplay = new GuiTextElement()
			    {
				    Text = Text,
				    Font = Alex.Font,
				    HorizontalAlignment = HorizontalAlignment.Center,
				    VerticalAlignment = VerticalAlignment.Bottom,
				    Scale = 0.50f,
				    Color = Color.White
				};
			    _progressBar.AddChild(_percentageDisplay);
		    }

		    public void UpdateProgress(int value)
		    {
			    _progressBar.Value = value;
			    _percentageDisplay.Text = $"{value}%";
			    _percentageDisplay.Y = _percentageDisplay.Height;
		    }
	    }
	}
}
