using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Input.Listeners;
using Alex.Entities;
using Alex.GameStates.Gui.InGame;
using Alex.GameStates.Playing;
using Alex.Gui.Elements;
using Alex.Gui.Elements.Inventory;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;

namespace Alex.GameStates.Hud
{
    public class PlayingHud : GuiScreen
    {
        private readonly GuiItemHotbar _hotbar;
        private readonly PlayerController _playerController;
	    public ChatComponent Chat;
		private PlayerInputManager InputManager => _playerController.InputManager;

		private Alex Alex { get; }

        public PlayingHud(Alex game, Player player, ChatComponent chat) : base()
        {
	        Alex = game;
            _playerController = player.Controller;
			InputManager.AddListener(new MouseInputListener(InputManager.PlayerIndex));

	        _hotbar = new GuiItemHotbar(player.Inventory);
	        _hotbar.Anchor = Alignment.BottomCenter;
	        _hotbar.Padding = Thickness.Zero;

			Chat = chat;
	        Chat.Anchor = Alignment.BottomLeft;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            AddChild(_hotbar);
            AddChild(new GuiCrosshair());
			AddChild(Chat);
        }

        protected override void OnUpdate(GameTime gameTime)
		{
			if (_playerController.MouseInputListener.IsButtonDown(MouseButton.ScrollUp))
			{
				if (Chat.Focused)
					Chat.ScrollUp();
				else
					_hotbar.SelectedIndex++;
			}

			if (_playerController.MouseInputListener.IsButtonDown(MouseButton.ScrollDown))
			{
				if (Chat.Focused)
					Chat.ScrollDown();
				else
					_hotbar.SelectedIndex--;
			}

			if (!Chat.Focused)
	        {
		        if (InputManager.IsPressed(InputCommand.HotBarSelect1)) _hotbar.SelectedIndex = 0;
		        if (InputManager.IsPressed(InputCommand.HotBarSelect2)) _hotbar.SelectedIndex = 1;
		        if (InputManager.IsPressed(InputCommand.HotBarSelect3)) _hotbar.SelectedIndex = 2;
		        if (InputManager.IsPressed(InputCommand.HotBarSelect4)) _hotbar.SelectedIndex = 3;
		        if (InputManager.IsPressed(InputCommand.HotBarSelect5)) _hotbar.SelectedIndex = 4;
		        if (InputManager.IsPressed(InputCommand.HotBarSelect6)) _hotbar.SelectedIndex = 5;
		        if (InputManager.IsPressed(InputCommand.HotBarSelect7)) _hotbar.SelectedIndex = 6;
		        if (InputManager.IsPressed(InputCommand.HotBarSelect8)) _hotbar.SelectedIndex = 7;
		        if (InputManager.IsPressed(InputCommand.HotBarSelect9)) _hotbar.SelectedIndex = 8;

		        if (InputManager.IsPressed(InputCommand.ToggleChat))
		        {
					Chat.Dismiss();
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

			base.OnUpdate(gameTime);
        }

        protected override void OnUpdateLayout()
		{
			base.OnUpdateLayout();
		}
    }
}
