using Alex.API.Data;
using Alex.API.Gui;
using Alex.API.Gui.Elements;

using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Hud;
using Alex.Gui.Elements.Inventory;
using Alex.Utils.Inventories;
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
        private readonly GuiItemHotbar _hotbar;
        private readonly PlayerController _playerController;
        private readonly HealthComponent _healthComponent;
        private readonly HungerComponent _hungerComponent;
        private readonly Container _healthContainer;
        private readonly MultiStackContainer _bottomContainer;
        private readonly TipPopupComponent _tipPopupComponent;
        private readonly StackContainer _healthAndHotbar;
        
	    public readonly ChatComponent Chat;
	    public readonly TitleComponent Title;
	    public readonly ScoreboardView Scoreboard;
        private PlayerInputManager InputManager => _playerController.InputManager;

		private Alex Alex { get; }
		private Player Player { get; }
        public PlayingHud(Alex game, Player player, TitleComponent titleComponent) : base()
        {
	        Title = titleComponent;

            Alex = game;
	        Player = player;
	        
	        Player.OnInventoryChanged += OnInventoryChanged;
	        
	        Anchor = Alignment.Fill;
	        Padding = Thickness.One;
	        
            _playerController = player.Controller;
			InputManager.AddListener(new MouseInputListener(InputManager.PlayerIndex));

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
			
	        _hotbar = new GuiItemHotbar(player.Inventory);
	        _hotbar.Anchor = Alignment.BottomCenter;
	        _hotbar.Padding = Thickness.Zero;

			Chat = new ChatComponent();
	        Chat.Enabled = false;
	        Chat.Anchor = Alignment.BottomLeft;

	        _healthContainer = new Container();
	        _healthContainer.Anchor = Alignment.Fill;

	        _healthContainer.Margin = new Thickness(0, 0, 0, 1);

	        _healthComponent = new HealthComponent(player);
	        _healthComponent.Anchor = Alignment.TopLeft;
	        
	        _hungerComponent = new HungerComponent(player);
	        _hungerComponent.Anchor = Alignment.TopRight;

	        _tipPopupComponent = new TipPopupComponent();
	        _tipPopupComponent.Anchor = Alignment.BottomCenter;
	        _tipPopupComponent.AutoSizeMode = AutoSizeMode.GrowAndShrink;
	        
	        Scoreboard = new ScoreboardView();
	        Scoreboard.Anchor = Alignment.MiddleRight;
        }

        private void OnInventoryChanged(object sender, Inventory e)
        {
	        _hotbar.Inventory = e;
        }

        private bool _didInit = false;
        protected override void OnInit(IGuiRenderer renderer)
        {
	        if (!_didInit)
	        {
		        _bottomContainer.AddChild(_tipPopupComponent);

		        _healthContainer.AddChild(_healthComponent);
		        _healthContainer.AddChild(_hungerComponent);

		        _healthAndHotbar.AddChild(_healthContainer);

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

		        AddChild(_bottomContainer);

		        AddChild(Chat);

		        //AddChild(_hotbar);
		        AddChild(new AlexCrosshair());
		        AddChild(Title);

		        AddChild(Scoreboard);

		        _didInit = true;
	        }
        }

        public bool CheckInput { get; set; } = true;
        protected override void OnUpdate(GameTime gameTime)
		{
			if (CheckInput)
			{
				if (!Chat.Focused)
				{
					Chat.Enabled = false;

					if (InputManager.IsPressed(AlexInputCommand.ToggleChat))
					{
						Chat.Dismiss();
						Chat.Enabled = true;
						Alex.GuiManager.FocusManager.FocusedElement = Chat;
					}

					if (InputManager.IsPressed(AlexInputCommand.Exit) && CheckInput)
					{
						//Player.Controller.CheckMovementInput = false;
						Alex.GameStateManager.SetActiveState<InGameMenuState>("ingamemenu");
					}
				}
				else if (CheckInput)
				{
					if (InputManager.IsPressed(AlexInputCommand.Exit))
					{
						Chat.Dismiss();
						Alex.GuiManager.FocusManager.FocusedElement = null;
					}

					if (InputManager.IsPressed(AlexInputCommand.Left))
					{
						Chat.MoveCursor(false);
					}
					else if (InputManager.IsPressed(AlexInputCommand.Right))
					{
						Chat.MoveCursor(true);
					}
				}
			}

			_hotbar.ShowItemCount = _hungerComponent.IsVisible = _healthComponent.IsVisible = Player.Gamemode != GameMode.Creative;
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
