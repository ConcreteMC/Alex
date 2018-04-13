using Microsoft.Xna.Framework;

namespace Alex.API.Gui
{
    public class GuiScreen : GuiElement
    {
        protected Game Game { get; }
        
        public GuiScreen(Game game)
        {
            BackgroundRepeatMode = TextureRepeatMode.Tile;
            Game = game;
        }

        public void UpdateSize(int width, int height)
        {
            Width = width;
            Height = height;

            UpdateLayout();
        }
    }
}
