using System.Linq;
using Alex.Common.Data;
using Alex.Common.Data.Options;
using Alex.Common.Gui.Elements;
using Alex.Common.Input;
using Alex.Entities;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Hud;
using Alex.Gui.Elements.Inventory;
using Alex.Gui.Elements.Map;
using Alex.Utils.Inventories;
using Alex.Worlds;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MiNET.Worlds;
using RocketUI;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace Alex.Gamestates.InGame.Hud
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
        
	    public readonly ChatComponent Chat;
	    public readonly TitleComponent Title;
	    public readonly ScoreboardView Scoreboard;
	    public readonly BossBarContainer BossBar;
        private PlayerInputManager PlayerInputManager => _playerController.InputManager;

		private Alex Alex { get; }
		private Player Player { get; }

		private OptionsPropertyAccessor<bool> _minimapAccessor;
		private OptionsPropertyAccessor<double> _minimapSizeAccessor;
		public PlayingHud(Alex game, World world, TitleComponent titleComponent) : base()
        {
	        Title = titleComponent;

            Alex = game;
	        Player = world.Player;
	        
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

			Chat = new ChatComponent();
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

	        Scoreboard = new ScoreboardView();
	        Scoreboard.Anchor = Alignment.MiddleRight;

	        _miniMap = new MapRenderElement(world)
	        {
		        Anchor = Alignment.TopRight
	        };

	        _minimapAccessor = Alex.Options.AlexOptions.MiscelaneousOptions.Minimap.Bind(OnMinimapEnabledChanged);
	        _miniMap.IsVisible = Alex.Options.AlexOptions.MiscelaneousOptions.Minimap.Value;

	        _minimapSizeAccessor = Alex.Options.AlexOptions.MiscelaneousOptions.MinimapSize.Bind(OnMinimapSizeChanged);
	        _miniMap.SetSize(Alex.Options.AlexOptions.MiscelaneousOptions.MinimapSize.Value);
        }

		private void OnMinimapSizeChanged(double oldvalue, double newvalue)
		{
			_miniMap.SetSize(newvalue);
		}

		private void OnMinimapEnabledChanged(bool oldvalue, bool newvalue)
		{
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

	        AddChild(Chat);

	        //AddChild(_hotbar);
	        AddChild(new AlexCrosshair());
	        AddChild(Title);

	        AddChild(Scoreboard);
		        
	        AddChild(BossBar);
	        
	        AddChild(_miniMap);

	        _didInit = true;
        }

        public bool CheckInput { get; set; } = true;
        protected override void OnUpdate(GameTime gameTime)
		{
			if (CheckInput)
			{
				if (!Chat.Focused)
				{
					Chat.Enabled = false;

					if (PlayerInputManager.IsPressed(AlexInputCommand.ToggleChat))
					{
						Chat.Dismiss();
						Chat.Enabled = true;
						Alex.GuiManager.FocusManager.FocusedElement = Chat;
					} 
					else if (PlayerInputManager.IsPressed(AlexInputCommand.Exit))
					{
						//Player.Controller.CheckMovementInput = false;
						Alex.GameStateManager.SetActiveState<InGameMenuState>("ingamemenu");
					}
				}
				else
				{
					if (PlayerInputManager.IsPressed(AlexInputCommand.Exit))
					{
						Chat.Dismiss();
						Alex.GuiManager.FocusManager.FocusedElement = null;
					}
				}
			}

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
	        Chat.Unload();
	        _minimapAccessor?.Dispose();
	        _minimapAccessor = null;
	        
	        _minimapSizeAccessor?.Dispose();
	        _minimapSizeAccessor = null;
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
