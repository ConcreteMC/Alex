using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Gamestates.Common;
using RocketUI;

namespace Alex.Gamestates
{
    public class LoadingWorldState : GuiMenuStateBase
    {
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

		public LoadingWorldState()
		{
			HeaderTitle.TranslationKey = "menu.loadingLevel";

		   _textDisplay = new GuiTextElement()
		    {
			    Text      = Text,
			    TextColor = TextColor.White,
					
			    Anchor    = Alignment.TopLeft,
			    HasShadow = false
		    };

		   _percentageDisplay = new GuiTextElement()
		    {
			    Text      = Text,
			    TextColor = TextColor.White,
					
			    Anchor    = Alignment.TopRight,
			    HasShadow = false
		    };

		    _progressBar = new GuiProgressBar()
		    {
			    Width  = 300,
			    Height = 9,
					
			    Anchor = Alignment.BottomFill,	
		    };

			var progContainer = new GuiContainer()
			{
				Width = 300,
				Height = 25,
			};

			progContainer.AddChild(_textDisplay);
			progContainer.AddChild(_percentageDisplay);
			progContainer.AddChild(_progressBar);

			var progressRow = Footer.AddRow(progContainer);

			UpdateProgress(LoadingState.ConnectingToServer, 10);
		}

	    public LoadingState CurrentState { get; private set; } = LoadingState.ConnectingToServer;
	    public int Percentage { get; private set; } = 0;
		public void UpdateProgress(LoadingState state, int percentage)
	    {
		    switch (state)
		    {
			    case LoadingState.ConnectingToServer:
				   // Text = "Connecting to server...";
				    _textDisplay.TranslationKey = "connect.connecting";
				    break;
			    case LoadingState.LoadingChunks:
				  //  Text = $"Loading chunks...";
				  _textDisplay.TranslationKey = "menu.loadingLevel";
				    break;
			    case LoadingState.GeneratingVertices:
				  //  Text = $"Building world...";
				  _textDisplay.TranslationKey = "menu.generatingTerrain";
				    break;
			    case LoadingState.Spawning:
				  //  Text = $"Getting ready...";
				  _textDisplay.TranslationKey = "connect.joining";
				    break;
		    }

		    UpdateProgress(percentage);
		    CurrentState = state;
		    Percentage = percentage;
	    }
	    public void UpdateProgress(int value)
	    {
		    _progressBar.Value      = value;
		    _percentageDisplay.Text = $"{value}%";
	    }
	}
}
