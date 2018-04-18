using System.Drawing;
using System.IO;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Graphics.Gui;
using Alex.Graphics.Gui.Elements;
using Alex.Utils;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.Gamestates
{
    public class LoadingWorldState : GameState
    {
		private LoadingWorldGui Screen { get; set; }
		public LoadingWorldState(Alex alex) : base(alex)
	    {
		    Gui = Screen = new LoadingWorldGui(Alex)
		    {
				DefaultBackgroundTexture = GuiTextures.OptionsBackground,
			    BackgroundRepeatMode = TextureRepeatMode.ScaleToFit
			};
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

	    private class LoadingWorldGui : GuiScreen
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
		    public LoadingWorldGui(Alex game) : base(game)
		    {
			    Alex = game;
		    }

		    protected override void OnInit(IGuiRenderer renderer)
		    {
				AddChild(_progressBarContainer = new GuiContainer()
			    {
				    Width = 300,
				    Height = 25,

				    Y = -25,
					
				    Anchor = Alignment.BottomCenter,
			    });

				_progressBarContainer.AddChild(_textDisplay = new GuiTextElement()
			    {
				    Text = Text,
				    TextColor = TextColor.Black,
					
				    Anchor = Alignment.TopLeft,
				    HasShadow = false
			    });

			    _progressBarContainer.AddChild(_percentageDisplay = new GuiTextElement()
			    {
				    Text = Text,
				    TextColor = TextColor.Black,
					
				    Anchor = Alignment.TopRight,
				    HasShadow = false
			    });

			    _progressBarContainer.AddChild(_progressBar = new GuiProgressBar()
			    {
				    Width = 300,
				    Height = 9,
					
				    Anchor = Alignment.BottomFill,
			    });
			}

		    public void UpdateProgress(int value)
		    {
			    _progressBar.Value = value;
			    _percentageDisplay.Text = $"{value}%";
			   // _percentageDisplay.Y = _percentageDisplay.Height;
		    }
	    }
	}
}
