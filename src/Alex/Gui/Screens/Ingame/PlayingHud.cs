using Alex.Common.Data;
using Alex.Common.Data.Options;
using Alex.Common.Gui;
using Alex.Common.Gui.Elements;
using Alex.Common.Input;
using Alex.Entities;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Hud;
using Alex.Gui.Elements.Inventory;
using Alex.Gui.Elements.Map;
using Alex.Gui.Elements.Scoreboard;
using Alex.Net;
using Alex.Utils.Inventories;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using MiNET.Worlds;
using RocketUI;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace Alex.Gui.Screens.Ingame
{
    public class PlayingHud : Screen, IChatRecipient
    {
	    private readonly MapRenderElement _miniMap;
	    
        private readonly GuiItemHotbar _hotbar;
        private readonly PlayerController _playerController;
        private readonly HealthComponent _healthComponent;
        private readonly HungerComponent _hungerComponent;
        private readonly AirComponent _airComponent;
        private readonly ExperienceComponent _experienceComponent;

        private readonly Container _armorAndAirContainer;
        private readonly Container _healthContainer;
        private readonly MultiStackContainer _bottomContainer;
        private readonly TipPopupComponent _tipPopupComponent;
        private readonly StackContainer _healthAndHotbar;
        private readonly StackContainer _rightSideStackContainer;
        
	    public readonly ChatComponent Chat;
	    public readonly TitleComponent Title;
	    public readonly ScoreboardView Scoreboard;
	    public readonly BossBarContainer BossBar;
        private PlayerInputManager PlayerInputManager => _playerController.InputManager;

		private Alex Alex { get; }
		private Player Player { get; }

		private OptionsPropertyAccessor<bool> _minimapAccessor;
		private OptionsPropertyAccessor<double> _minimapSizeAccessor;
		private OptionsPropertyAccessor<int> _renderDistanceAccessor;
		private OptionsPropertyAccessor<bool> _hudVisibleAccessor;
		private OptionsPropertyAccessor<bool> _chatVisibleAccessor;
		private OptionsPropertyAccessor<int> _chatHistoryAccessor;
		private OptionsPropertyAccessor<ZoomLevel> _zoomLevelAccessor;

		private InputActionBinding _chatToggleBinding;
		public PlayingHud(Alex game, World world, TitleComponent titleComponent, NetworkProvider networkProvider) : base()
        {
	        Title = titleComponent;

            Alex = game;
	        Player = world.Player;
	        
	        var options = Alex.Options.AlexOptions;
	        Player.OnInventoryChanged += OnInventoryChanged;
	        
	        Anchor = Alignment.Fill;
	        Padding = Thickness.One * 3;
	        
            _playerController = Player.Controller;
            PlayerInputManager.AddListener(new MouseInputListener(PlayerInputManager.PlayerIndex));

            _healthAndHotbar = new StackContainer()
			{
				Orientation = Orientation.Vertical,
				ChildAnchor = Alignment.Fill
			};
			
			_bottomContainer = new MultiStackContainer();
			_bottomContainer.ChildAnchor = Alignment.BottomFill;
			_bottomContainer.Anchor = Alignment.BottomCenter;
			_bottomContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			_bottomContainer.Orientation = Orientation.Vertical;

			//BottomContainer.
			
	        _hotbar = new GuiItemHotbar(Player.Inventory);
	        _hotbar.Anchor = Alignment.BottomCenter;
	        _hotbar.Padding = Thickness.Zero;

			Chat = new ChatComponent(networkProvider);
	        Chat.Enabled = false;
	        Chat.Anchor = Alignment.BottomLeft;
				
	        _healthContainer = new Container();
	        _healthContainer.Anchor = Alignment.Fill;

	        _healthContainer.Margin = new Thickness(0, 0, 0, 1);

	        _healthComponent = new HealthComponent(Player);
	        _healthComponent.Anchor = Alignment.TopLeft;
	        
	        _hungerComponent = new HungerComponent(Player);
	        _hungerComponent.Anchor = Alignment.TopRight;

	        _armorAndAirContainer = new Container();
	        _armorAndAirContainer.Anchor = Alignment.Fill;
	        _armorAndAirContainer.Margin = new Thickness(0, 0, 0, 1);
	        
	        _airComponent = new AirComponent(Player);
	        _airComponent.Anchor = Alignment.TopRight;
	        
	        _experienceComponent = new ExperienceComponent(Player);
	        _experienceComponent.Margin = new Thickness(0, 0, 0, 1);
	        _experienceComponent.Anchor = Alignment.BottomFill;

	        _tipPopupComponent = new TipPopupComponent();
	        _tipPopupComponent.Anchor = Alignment.BottomCenter;
	        _tipPopupComponent.AutoSizeMode = AutoSizeMode.GrowAndShrink;

	        BossBar = new BossBarContainer();

	        Scoreboard = new ScoreboardView()
	        {
		        Anchor = Alignment.MiddleRight,
		        Margin = new Thickness(0, 5, 0, 0)
	        };
	      //  Scoreboard.Anchor = Alignment.MiddleRight;

	      _miniMap = new MapRenderElement(world.Map)
	        {
		        Anchor = Alignment.TopRight,
		        ZoomLevel = ZoomLevel.Level8,
		        FixedRotation = false,
		        CanFocus = false,
		        Enabled = false,
		        CanHighlight = false
	        };

	        _rightSideStackContainer = new StackContainer()
	        {
		        Orientation = Orientation.Vertical, 
		        Anchor = Alignment.FillRight,
		        ChildAnchor = Alignment.Default
	        };

	        options.UserInterfaceOptions.Scoreboard.Position.Bind(SetScoreboardPosition);
	        SetScoreboardPosition(ElementPosition.Default, options.UserInterfaceOptions.Scoreboard.Position.Value);
	        
	        _renderDistanceAccessor = options.VideoOptions.RenderDistance.Bind(RenderDistanceChanged);
	        _miniMap.Radius = _renderDistanceAccessor.Value;

	        _minimapAccessor = options.UserInterfaceOptions.Minimap.Enabled.Bind(OnMinimapEnabledChanged);
	        OnMinimapEnabledChanged(false, _minimapAccessor.Value);
	        //_miniMap.IsVisible = _minimapAccessor.Value;

	        _minimapSizeAccessor = options.UserInterfaceOptions.Minimap.Size.Bind(OnMinimapSizeChanged);
	        _miniMap.SetSize(_minimapSizeAccessor.Value);

	        options.UserInterfaceOptions.Scoreboard.Position.Bind(SetMinimapPosition);
	        SetMinimapPosition(ElementPosition.RightTop, options.UserInterfaceOptions.Minimap.Position.Value);
	        
	        _zoomLevelAccessor = options.UserInterfaceOptions.Minimap.DefaultZoomLevel.Bind(OnZoomLevelChanged);
	        _miniMap.ZoomLevel = _zoomLevelAccessor.Value;
	        
	        _hudVisibleAccessor = options.VideoOptions.DisplayHud.Bind(DisplayHudValueChanged);
	        IsVisible = _hudVisibleAccessor.Value;
	        //IsVisible = 
	        
	        _chatToggleBinding = PlayerInputManager.RegisterListener(
		        AlexInputCommand.ToggleChat, InputBindingTrigger.Discrete, ToggleChat);

	        _chatVisibleAccessor = options.UserInterfaceOptions.Chat.Enabled.Bind(SetChatVisibility);
	        Chat.IsVisible = _chatVisibleAccessor.Value;
	        
	        _chatHistoryAccessor = options.UserInterfaceOptions.Chat.MessageHistory.Bind(SetChatHistorySize);
	        Chat.SetHistorySize(_chatHistoryAccessor.Value);
        }
		
		private void SetChatHistorySize(int oldvalue, int newvalue)
		{
			Chat.SetHistorySize(newvalue);
		}
		
		private void SetChatVisibility(bool oldvalue, bool newvalue)
		{
			Chat.IsVisible = newvalue;
		}

		private void SetScoreboardPosition(ElementPosition oldvalue, ElementPosition newvalue)
		{
			Alignment alignment = newvalue switch
			{
				ElementPosition.RightMiddle => Alignment.MiddleRight,
				ElementPosition.RightTop    => Alignment.TopRight,
				ElementPosition.RightBottom => Alignment.BottomRight,
				ElementPosition.LeftTop     => Alignment.TopLeft,
				ElementPosition.LeftMiddle  => Alignment.MiddleLeft,
				ElementPosition.LeftBottom  => Alignment.BottomLeft,
				_                           => Scoreboard.Anchor
			};

			Scoreboard.Anchor = alignment;
		}
		
		private void SetMinimapPosition(ElementPosition oldvalue, ElementPosition newvalue)
		{
			Alignment alignment = newvalue switch
			{
				ElementPosition.RightMiddle => Alignment.MiddleRight,
				ElementPosition.RightTop    => Alignment.TopRight,
				ElementPosition.RightBottom => Alignment.BottomRight,
				ElementPosition.LeftTop     => Alignment.TopLeft,
				ElementPosition.LeftMiddle  => Alignment.MiddleLeft,
				ElementPosition.LeftBottom  => Alignment.BottomLeft,
				_                           => Scoreboard.Anchor
			};

			_miniMap.Anchor = alignment;
		}

		private void ToggleChat()
		{
			if (!Chat.IsVisible)
				return;
			
			if (!CheckInput)
				return;
			
			if (!Chat.Focused && Chat.RootScreen?.GuiManager != null)
			{
				Chat.Dismiss();
				Chat.Enabled = true;
				Alex.GuiManager.FocusManager.FocusedElement = Chat;
			}
			else
			{
				//Chat.Dismiss();
				//Alex.GuiManager.FocusManager.FocusedElement = null;
			}
		}

		private void OnZoomLevelChanged(ZoomLevel oldvalue, ZoomLevel newvalue)
		{
			_miniMap.ZoomLevel = newvalue;
		}

		private void DisplayHudValueChanged(bool oldvalue, bool newvalue)
		{
			IsVisible = newvalue;
		}

		private void RenderDistanceChanged(int oldvalue, int newvalue)
		{
			_miniMap.Radius = newvalue;
		}

		private void OnMinimapSizeChanged(double oldvalue, double newvalue)
		{
			_miniMap.SetSize(newvalue);
		}

		private void OnMinimapEnabledChanged(bool oldvalue, bool newvalue)
		{
			if (newvalue)
			{
				_rightSideStackContainer.ChildAnchor = Alignment.Default;
			}
			else
			{
				_rightSideStackContainer.ChildAnchor = Alignment.MiddleCenter;
			}
			
			_miniMap.IsVisible = newvalue;
		}

		private void OnInventoryChanged(object sender, Inventory e)
        {
	        _hotbar.Inventory = e;
        }

        private bool _didInit = false;

        protected override void OnInit(IGuiRenderer renderer)
        {
	        if (_didInit) return;

	        _armorAndAirContainer.AddChild(_airComponent);
	        _healthAndHotbar.AddChild(_armorAndAirContainer);
		        
	        _healthContainer.AddChild(_healthComponent);
	        _healthContainer.AddChild(_hungerComponent);

	        _healthAndHotbar.AddChild(_healthContainer);

	        _healthAndHotbar.AddChild(_experienceComponent);
	        _healthAndHotbar.AddChild(_hotbar);

	        _bottomContainer.AddRow(
		        container =>
		        {
			        //		        container.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			        container.Anchor = Alignment.BottomCenter;
			        container.ChildAnchor = Alignment.FillCenter;

			        container.AddChild(_healthAndHotbar);
			        //container.AddChild(_hotbar);
		        });

	        AddChild(_tipPopupComponent);
	        
	        AddChild(_bottomContainer);
	        
	        _rightSideStackContainer.AddChild(_miniMap);
	        _rightSideStackContainer.AddChild(Scoreboard);
	        AddChild(_rightSideStackContainer);

	        AddChild(Chat);

	        //AddChild(_hotbar);
	        AddChild(new AlexCrosshair());
	        AddChild(Title);

	        //AddChild(Scoreboard);
		        
	        AddChild(BossBar);
	        
	       // AddChild(_miniMap);

	        _didInit = true;
        }

        public bool CheckInput { get; set; } = true;
        protected override void OnUpdate(GameTime gameTime)
		{
			_experienceComponent.IsVisible = _hotbar.ShowItemCount = _hungerComponent.IsVisible = _healthComponent.IsVisible = Player.Gamemode != GameMode.Creative;

			int offset = 0;
			if (Player.Gamemode == GameMode.Creative)
			{
				offset = RenderBounds.Bottom - _hotbar.RenderBounds.Top;
			}
			else
			{
				if (_airComponent.IsVisible)
				{
					offset = RenderBounds.Bottom - _airComponent.RenderBounds.Top;
				}
				else
				{
					offset = RenderBounds.Bottom - _healthComponent.RenderBounds.Top;
				}
			}

			if (offset != _tipPopupComponent.Margin.Bottom) //We don't wanna cause an UpdateLayout call every frame.
			{
				_tipPopupComponent.Margin = new Thickness(0, 0, 0, offset);
			}
			//if (Player.Gamemode != Gamemode.Creative){}

			base.OnUpdate(gameTime);
        }

        protected override void OnUpdateLayout()
		{
			base.OnUpdateLayout();
		}

        public void Unload()
        {
	        if (_chatToggleBinding != null)
	        {
		        PlayerInputManager.UnregisterListener(_chatToggleBinding);
		        _chatToggleBinding = null;
	        }

	        Chat.Unload();
	        _minimapAccessor?.Dispose();
	        _minimapAccessor = null;
	        
	        _minimapSizeAccessor?.Dispose();
	        _minimapSizeAccessor = null;
	        
	        _renderDistanceAccessor?.Dispose();
	        _renderDistanceAccessor = null;
	        
	        _hudVisibleAccessor?.Dispose();
	        _hudVisibleAccessor = null;
	        
	        _zoomLevelAccessor?.Dispose();
	        _zoomLevelAccessor = null;
	        
	        _chatVisibleAccessor?.Dispose();
	        _chatVisibleAccessor = null;
	        
	        _chatHistoryAccessor?.Dispose();
	        _chatHistoryAccessor = null;
        }

        /// <inheritdoc />
        public void AddMessage(string message, MessageType type)
        {
	        if (type == MessageType.Raw || type == MessageType.Chat || type == MessageType.Whisper
	            || type == MessageType.Announcement || type == MessageType.System)
	        {
		        Chat?.AddMessage(message, type);
	        }
	        else
	        {
		        _tipPopupComponent?.AddMessage(message, type);
	        }
        }
    }
}
