using Alex.API.Data;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Rendering;
using Alex.API.Input;
using Alex.Gamestates.Gui;
using Alex.Gamestates.Playing;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates.Hud
{
    public class PlayingHud : GuiScreen
    {
        private GuiItemHotbar _hotbar;
        private PlayerController _playerController;
	    public ChatComponent Chat;
		private PlayerInputManager InputManager => _playerController.InputManager;

        public PlayingHud(Game game, PlayerController playerController, ChatComponent chat) : base(game)
        {
            _playerController = playerController;
            _hotbar = new GuiItemHotbar();
	        _hotbar.Anchor = Alignment.BottomCenter;

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
	        if (!Chat.RenderChatInput)
	        {
				if (InputManager.IsPressed(InputCommand.HotBarSelectNext))
		        {
			        _hotbar.SelectedIndex++;
		        }

		        if (InputManager.IsPressed(InputCommand.HotBarSelectPrevious))
		        {
			        _hotbar.SelectedIndex--;
		        }

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
			        Chat.ToggleInput();
		        }
			}
	        else
	        {
		        if (InputManager.IsPressed(InputCommand.ToggleMenu))
		        {

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
