using Alex.API.GameStates;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Gamestates.Common;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Gamestates
{
    public class LoadingWorldState : GuiInGameStateBase
    {
	    private static readonly Logger    Log = LogManager.GetCurrentClassLogger(typeof(LoadingWorldState));
	    
	    private readonly        GuiProgressBar _progressBar;
	    private readonly        GuiTextElement _textDisplay;
	    private readonly        GuiTextElement _subTextDisplay;
	    private readonly        GuiTextElement _percentageDisplay;
		
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
			    _subTextDisplay.Text = value ?? string.Empty;
		    }
	    }

	    private bool _connectingToServer = false;

	    public bool ConnectingToServer
	    {
		    get
		    {
			    return _connectingToServer;
		    }
		    set
		    {
			    _connectingToServer = value;
			    UpdateProgress(CurrentState, Percentage, SubText);
		    }
	    }

	    public LoadingWorldState(IGameState parent = null)
		{
			GuiStackContainer progressBarContainer;

			AddChild(progressBarContainer = new GuiStackContainer()
			{
				//Width  = 300,
				//Height = 35,
				//Margin = new Thickness(12),
				
				Anchor = Alignment.MiddleCenter,
				Background = Color.Transparent,
				BackgroundOverlay = Color.Transparent,
				Orientation = Orientation.Vertical
			});
			
			if (parent == null)
			{
				Background = new GuiTexture2D
				{ 
					TextureResource = GuiTextures.OptionsBackground, 
					RepeatMode = TextureRepeatMode.Tile,
					Scale =  new Vector2(2f, 2f),
				};
				
				BackgroundOverlay = new Color(Color.Black, 0.65f);
			}
			else
			{
				ParentState = parent;
				HeaderTitle.IsVisible = false;
			}
			
			progressBarContainer.AddChild(_textDisplay = new GuiTextElement()
			{
				Text      = Text,
				TextColor = TextColor.White,
				
				Anchor    = Alignment.TopCenter,
				HasShadow = false,
				Scale = 1.5f
			});

			GuiElement element;

			progressBarContainer.AddChild(element = new GuiElement()
			{
				Width  = 300,
				Height = 35,
				Margin = new Thickness(12),
			});
			
			element.AddChild(_percentageDisplay = new GuiTextElement()
			{
				Text      = Text,
				TextColor = TextColor.White,
				
				Anchor    = Alignment.TopRight,
				HasShadow = false
			});

			element.AddChild(_progressBar = new GuiProgressBar()
			{
				Width  = 300,
				Height = 9,
				
				Anchor = Alignment.MiddleCenter,
			});

			progressBarContainer.AddChild(
				_subTextDisplay = new GuiTextElement()
				{
					Text = Text, TextColor = TextColor.White, Anchor = Alignment.BottomLeft, HasShadow = false
				});

			HeaderTitle.TranslationKey = "menu.loadingLevel";

			UpdateProgress(LoadingState.ConnectingToServer, 10);
		}

	    public LoadingState CurrentState { get; private set; } = LoadingState.ConnectingToServer;
	    public int Percentage { get; private set; } = 0;
		public void UpdateProgress(LoadingState state, int percentage, string substring = null)
	    {
		    switch (state)
		    {
			    case LoadingState.ConnectingToServer:
				   // Text = "Connecting to server...";
				    _textDisplay.TranslationKey = "connect.connecting";
				    break;
			    case LoadingState.LoadingChunks:
				  //  Text = $"Loading chunks...";
				  _textDisplay.TranslationKey = _connectingToServer ? "multiplayer.downloadingTerrain" : "menu.loadingLevel";
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
		    SubText = substring;
	    }
		
	    public void UpdateProgress(int value)
	    {
		    _progressBar.Value      = value;
		    _percentageDisplay.Text = $"{value}%";
	    }

	    /// <inheritdoc />
	    protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	    {
		    if (ParentState is IGuiElement gui)
		    {
			    gui.Draw(graphics, gameTime);
		    }

		    base.OnDraw(graphics, gameTime);
	    }

	    /// <inheritdoc />
	    protected override void OnUpdateLayout()
	    {
		    if (ParentState is IGuiElement gui)
		    {
			    gui.InvalidateLayout();
		    }
		    
		    base.OnUpdateLayout();
	    }
    }
}
