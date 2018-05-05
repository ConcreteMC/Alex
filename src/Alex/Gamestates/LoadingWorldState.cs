using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Alex.API.World;
using Alex.GameStates.Gui.Common;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Elements.Layout;

namespace Alex.GameStates
{
    public class LoadingWorldState : GuiMenuStateBase
    {
	    private readonly GuiContainer   _progressBarContainer;
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

		public LoadingWorldState()
	    {
		    AddGuiElement(_progressBarContainer = new GuiContainer()
		    {
			    Width  = 300,
			    Height = 25,

			    Y = -25,
					
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
					
			    Anchor = Anchor.BottomFill,
		    });
	    }
		
	    public void UpdateProgress(LoadingState state, int percentage)
	    {
		    switch (state)
		    {
			    case LoadingState.ConnectingToServer:
				    Text = "Connecting to server...";
				    break;
			    case LoadingState.LoadingChunks:
				    Text = $"Loading chunks...";
				    break;
			    case LoadingState.GeneratingVertices:
				    Text = $"Building world...";
				    break;
			    case LoadingState.Spawning:
				    Text = $"Getting ready...";
				    break;
		    }

		    UpdateProgress(percentage);
	    }
	    public void UpdateProgress(int value)
	    {
		    _progressBar.Value      = value;
		    _percentageDisplay.Text = $"{value}%";
	    }
	}
}
