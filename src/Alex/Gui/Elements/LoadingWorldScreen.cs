using System;
using Alex.Common.Gui.Elements;
using Alex.Common.Gui.Graphics;
using Alex.Common.Utils;
using Alex.Common.World;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gui.Elements
{
    public class LoadingWorldScreen : Screen
    {
	    private static readonly Logger    Log = LogManager.GetCurrentClassLogger(typeof(LoadingWorldScreen));
	    
	    private readonly SimpleProgressBar _progressBar;
	    private readonly TextElement _textDisplay;
	    private readonly TextElement _subTextDisplay;
	    private readonly TextElement _percentageDisplay;
	    private readonly Button      _cancelButton;
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
			    _cancelButton.IsVisible = value;
		    }
	    }

	    public LoadingWorldScreen()
		{
			StackContainer progressBarContainer;

			AddChild(new Image(AlexGuiTextures.AlexLogo)
			{
				Margin = new Thickness(0, 25, 0, 0),
				Anchor = Alignment.TopCenter
			});
			
			AddChild(progressBarContainer = new StackContainer()
			{
				//Width  = 300,
				//Height = 35,
				//Margin = new Thickness(12),
				
				Anchor = Alignment.MiddleCenter,
				Background = Color.Transparent,
				BackgroundOverlay = Color.Transparent,
				Orientation = Orientation.Vertical
			});
			
			BackgroundOverlay = new Color(Color.Black, 0.65f);
			
			/*if (parent == null)
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
			}*/
			
			progressBarContainer.AddChild(_textDisplay = new TextElement()
			{
				Text      = Text,
				TextColor = (Color) TextColor.White,
				
				Anchor    = Alignment.TopCenter,
				HasShadow = false,
				Scale = 1.5f
			});

			RocketElement element;

			progressBarContainer.AddChild(element = new RocketElement()
			{
				Width  = 300,
				Height = 35,
				Margin = new Thickness(12),
			});
			
			element.AddChild(_percentageDisplay = new TextElement()
			{
				Text      = Text,
				TextColor = (Color) TextColor.White,
				
				Anchor    = Alignment.TopRight,
				HasShadow = false
			});

			element.AddChild(_progressBar = new SimpleProgressBar()
			{
				Width  = 300,
				Height = 9,
				
				Anchor = Alignment.MiddleCenter,
			});

			progressBarContainer.AddChild(
				_subTextDisplay = new TextElement()
				{
					Text = Text, TextColor = (Color) TextColor.White, Anchor = Alignment.BottomLeft, HasShadow = false
				});

			AddChild(_cancelButton = new AlexButton("Cancel", Cancel)
			{
				Anchor = Alignment.TopLeft
			});
			
			//HeaderTitle.TranslationKey = "menu.loadingLevel";

			UpdateProgress(LoadingState.ConnectingToServer, 10);
		}

	    private void Cancel()
	    {
		    CancelAction?.Invoke();
	    }

	    public LoadingState CurrentState { get; private set; } = LoadingState.ConnectingToServer;
	    public int          Percentage   { get; private set; } = 0;
	    public Action       CancelAction { get; set; }         = null;

	    public void UpdateProgress(LoadingState state, int percentage, string substring = null)
	    {
		    switch (state)
		    {
			    case LoadingState.LoadingResources:
				    _textDisplay.TranslationKey = "resourcepack.loading";
				    break;
			    case LoadingState.RetrievingResources:
				    _textDisplay.TranslationKey = "resourcepack.downloading";
				    break;
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
		    ParentElement?.Draw(graphics, gameTime);
		    base.OnDraw(graphics, gameTime);
	    }

	    /// <inheritdoc />
	    protected override void OnUpdateLayout()
	    {
		    ParentElement?.InvalidateLayout();
		    
		    base.OnUpdateLayout();
	    }
    }
}
