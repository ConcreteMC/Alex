using Alex.API.Events;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Input.Listeners;
using Alex.API.Utils;
using Alex.Entities;
using Alex.GameStates.Gui.InGame;
using Alex.GameStates.Playing;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Inventory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.GameStates.Hud
{
    public class PlayingHud : GuiScreen
    {
        private readonly GuiItemHotbar _hotbar;
        private readonly PlayerController _playerController;
        private readonly HealthComponent _healthComponent;
        private readonly HungerComponent _hungerComponent;
        private readonly GuiContainer _healthContainer;
        private readonly GuiMultiStackContainer _bottomContainer;
        private readonly TipPopupComponent _tipPopupComponent;
        private readonly GuiStackContainer _healthAndHotbar;
        
	    public readonly ChatComponent Chat;
	    public readonly TitleComponent Title;
        private PlayerInputManager InputManager => _playerController.InputManager;

		private Alex Alex { get; }
		private Player Player { get; }
        public PlayingHud(Alex game, Player player, TitleComponent titleComponent) : base()
        {
	        Title = titleComponent;

            Alex = game;
	        Player = player;
	        
	        Anchor = Alignment.Fill;
	        Padding = Thickness.One;
	        
            _playerController = player.Controller;
			InputManager.AddListener(new MouseInputListener(InputManager.PlayerIndex));

			_healthAndHotbar = new GuiStackContainer()
			{
				Orientation = Orientation.Vertical,
				ChildAnchor = Alignment.Fill
			};
			
			_bottomContainer = new GuiMultiStackContainer();
			_bottomContainer.ChildAnchor = Alignment.BottomFill;
			_bottomContainer.Anchor = Alignment.BottomCenter;
			_bottomContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			_bottomContainer.Orientation = Orientation.Vertical;

			//BottomContainer.
			
	        _hotbar = new GuiItemHotbar(player.Inventory);
	        _hotbar.Anchor = Alignment.BottomCenter;
	        _hotbar.Padding = Thickness.Zero;

			Chat = new ChatComponent(game.Services.GetRequiredService<IEventDispatcher>());
	        Chat.Enabled = false;
	        Chat.Anchor = Alignment.BottomLeft;

	        _healthContainer = new GuiContainer();
	        _healthContainer.Anchor = Alignment.Fill;
	       // _healthContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
	        
	        _healthContainer.Margin = new Thickness(0, 0, 0, 1);
	     //   _healthContainer.Orientation = Orientation.Horizontal;
	      //  _healthContainer.ChildAnchor = Alignment.None;

	      _healthComponent = new HealthComponent(player);
	        _healthComponent.Anchor = Alignment.TopLeft;
	        
	        _hungerComponent = new HungerComponent(player);
	        _hungerComponent.Anchor = Alignment.TopRight;

	        _tipPopupComponent = new TipPopupComponent();
	        _tipPopupComponent.Anchor = Alignment.BottomCenter;
	        _tipPopupComponent.AutoSizeMode = AutoSizeMode.GrowAndShrink;
	        //_healthComponent.Anchor
	        //  _hotbar.AddChild(_healthComponent = new HealthComponent(player));
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
	        Alex.Services.GetRequiredService<IEventDispatcher>().RegisterEvents(_tipPopupComponent);
	        //_healthContainer.AddChild(_healthComponent);
	       // _healthContainer.AddChild(_hungerComponent);
	        
	      //  _bottomContainer.AddChild(_healthContainer);
	     /*   _bottomContainer.AddRow(row =>
	        {
		        row.Anchor = Alignment.Fill;
		        row.ChildAnchor = Alignment.Fill;

		        //row.AddChild(_healthContainer);

		        t
	        });*/

	     _bottomContainer.AddChild(_tipPopupComponent);
	     
	     _healthContainer.AddChild(_healthComponent);
	     _healthContainer.AddChild(_hungerComponent);
	     
	     _healthAndHotbar.AddChild(_healthContainer);
	     
	     _healthAndHotbar.AddChild(_hotbar);

	     _bottomContainer.AddRow(container =>
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
            AddChild(new GuiCrosshair());
            AddChild(Title);
        }

        public bool CheckInput { get; set; } = true;
        protected override void OnUpdate(GameTime gameTime)
		{
			if (CheckInput)
			{
				if (_playerController.MouseInputListener.IsButtonDown(MouseButton.ScrollUp))
				{
					//if (Chat.Focused)
					//	Chat.ScrollUp();
					//else
						Player.Inventory.SelectedSlot--;
				}

				if (_playerController.MouseInputListener.IsButtonDown(MouseButton.ScrollDown))
				{
					//if (Chat.Focused)
					//	Chat.ScrollDown();
					//else
						Player.Inventory.SelectedSlot++;
				}

				if (!Chat.Focused)
				{
					Chat.Enabled = false;
					if (InputManager.IsPressed(InputCommand.HotBarSelect1)) Player.Inventory.SelectedSlot = 0;
					if (InputManager.IsPressed(InputCommand.HotBarSelect2)) Player.Inventory.SelectedSlot = 1;
					if (InputManager.IsPressed(InputCommand.HotBarSelect3)) Player.Inventory.SelectedSlot = 2;
					if (InputManager.IsPressed(InputCommand.HotBarSelect4)) Player.Inventory.SelectedSlot = 3;
					if (InputManager.IsPressed(InputCommand.HotBarSelect5)) Player.Inventory.SelectedSlot = 4;
					if (InputManager.IsPressed(InputCommand.HotBarSelect6)) Player.Inventory.SelectedSlot = 5;
					if (InputManager.IsPressed(InputCommand.HotBarSelect7)) Player.Inventory.SelectedSlot = 6;
					if (InputManager.IsPressed(InputCommand.HotBarSelect8)) Player.Inventory.SelectedSlot = 7;
					if (InputManager.IsPressed(InputCommand.HotBarSelect9)) Player.Inventory.SelectedSlot = 8;

					if (InputManager.IsPressed(InputCommand.ToggleChat))
					{
						Chat.Dismiss();
						Chat.Enabled = true;
						Alex.GuiManager.FocusManager.FocusedElement = Chat;
					}

					if (InputManager.IsPressed(InputCommand.Exit) && CheckInput)
					{
						Alex.GameStateManager.SetActiveState<InGameMenuState>("ingamemenu");
					}
				}
				else if (CheckInput)
				{
					if (InputManager.IsPressed(InputCommand.Exit))
					{
						Chat.Dismiss();
						Alex.GuiManager.FocusManager.FocusedElement = null;
					}

					if (InputManager.IsPressed(InputCommand.Left))
					{
						Chat.MoveCursor(false);
					}
					else if (InputManager.IsPressed(InputCommand.Right))
					{
						Chat.MoveCursor(true);
					}
				}
			}

			_healthComponent.IsVisible = Player.Gamemode != Gamemode.Creative;
			
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
	        
	        Alex.Services.GetRequiredService<IEventDispatcher>().UnregisterEvents(_tipPopupComponent);
        }
    }
}
