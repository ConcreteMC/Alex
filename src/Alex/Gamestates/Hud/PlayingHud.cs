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
        private readonly GuiMultiStackContainer BottomContainer;
        
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

            _playerController = player.Controller;
			InputManager.AddListener(new MouseInputListener(InputManager.PlayerIndex));

			BottomContainer = new GuiMultiStackContainer();
			BottomContainer.ChildAnchor = Alignment.BottomLeft;
			BottomContainer.Anchor = Alignment.BottomCenter;
			BottomContainer.Orientation = Orientation.Vertical;
			BottomContainer.Padding = new Thickness(0, 0, 0, 2);
			//BottomContainer.
			
	        _hotbar = new GuiItemHotbar(player.Inventory);
	        _hotbar.Anchor = Alignment.BottomCenter;
	        _hotbar.Padding = Thickness.Zero;

			Chat = new ChatComponent(game.Services.GetRequiredService<IEventDispatcher>());
	        Chat.Enabled = false;
	        Chat.Anchor = Alignment.BottomLeft;

	        _healthComponent = new HealthComponent(player);
			//_healthComponent.Anchor
	        //  _hotbar.AddChild(_healthComponent = new HealthComponent(player));
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
	        AddChild(BottomContainer);
	        BottomContainer.AddRow(container =>
	        {
		        container.Anchor = Alignment.BottomLeft;
		        //container.Padding = new Thickness(0, 0, 0, 4);
		        container.AddChild(_healthComponent);
	        });
	        BottomContainer.AddRow(container =>
	        {
		        container.Margin = new Thickness(2, 2);
		        container.Anchor = Alignment.BottomCenter;
		        container.AddChild(_hotbar);
	        });
	        //AddChild(_hotbar);
            AddChild(new GuiCrosshair());
			AddChild(Chat);
			AddChild(Title);
        }

        protected override void OnUpdate(GameTime gameTime)
		{
			if (_playerController.MouseInputListener.IsButtonDown(MouseButton.ScrollUp))
			{
				if (Chat.Focused)
					Chat.ScrollUp();
				else
					Player.Inventory.SelectedSlot--;
			}

			if (_playerController.MouseInputListener.IsButtonDown(MouseButton.ScrollDown))
			{
				if (Chat.Focused)
					Chat.ScrollDown();
				else
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

		        if (InputManager.IsPressed(InputCommand.ToggleMenu))
		        {
			        Alex.GameStateManager.SetActiveState<InGameMenuState>("ingamemenu");
				}
			}
	        else
	        {
		        if (InputManager.IsPressed(InputCommand.ToggleMenu))
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
        }
    }
}
