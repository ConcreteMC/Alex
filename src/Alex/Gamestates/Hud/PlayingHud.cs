using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Rendering;
using Alex.API.Input;
using Alex.Gamestates.Gui;
using Alex.Gamestates.Playing;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates.Hud
{
    public class PlayingHud : GuiScreen
    {
        private GuiItemHotbar _hotbar;
        private PlayerController _playerController;
        private PlayerInputManager InputManager => _playerController.InputManager;

        public PlayingHud(Game game, PlayerController playerController) : base(game)
        {
            DebugColor = Color.Green;
            _playerController = playerController;
            _hotbar = new GuiItemHotbar();
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            AddChild(_hotbar);
            AddChild(new GuiCrosshair());
        }

        protected override void OnUpdate(GameTime gameTime)
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
        }

        protected override void OnUpdateLayout()
        {
            _hotbar.X = (int)((Width - (float)_hotbar.Width) / 2f) - 1;
            _hotbar.Y = Height - _hotbar.Height - 1;
        }
    }
}
