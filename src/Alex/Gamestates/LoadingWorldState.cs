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
		private Texture2D Background { get; }
		private LoadingWorldGui Screen { get; set; }
		public LoadingWorldState(Alex alex) : base(alex)
	    {
		    Screen = new LoadingWorldGui(Alex)
		    {
				DefaultBackgroundTexture = GuiTextures.TitleScreenBackground
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

	    protected override void OnLoad(RenderArgs args)
	    {
		    base.OnLoad(args);
		}

	    protected override void OnShow()
	    {
		    base.OnShow();
	    }

	    private class LoadingWorldGui : GuiScreen
	    {
		    private GuiStackContainer _progressBarContainer;
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
			    AddChild(new GuiImage(GuiTextures.AlexLogo)
			    {
				    Height = 50,
					Width = 150,
					//X = -75,
					Y = -25,
				    HorizontalAlignment = HorizontalAlignment.Center,
				    VerticalAlignment = VerticalAlignment.Center
			    });

				AddChild(_progressBarContainer = new GuiStackContainer()
				{
					Width  = 300,
					Height = 9,
					Y = -50,

					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment   = VerticalAlignment.Bottom,
				});
				
			    _progressBarContainer.AddChild(_textDisplay = new GuiTextElement()
			    {
				    Text                = Text,
				    TextColor           = TextColor.White
			    });

			    _progressBarContainer.AddChild(_progressBar = new GuiProgressBar()
			    {
					HorizontalAlignment = HorizontalAlignment.Stretch
			    });
				
			    _progressBarContainer.AddChild(_percentageDisplay = new GuiTextElement()
			    {
				    Text = Text,
				    TextColor = TextColor.White
				});
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
