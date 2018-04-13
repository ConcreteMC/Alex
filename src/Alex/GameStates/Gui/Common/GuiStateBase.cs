using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Alex.Gamestates;

namespace Alex.GameStates.Gui.Common
{
    public class GuiStateBase : GameState
    {

        public GuiStateBase(Alex alex) : base(alex)
        {
            Gui = new GuiScreen(alex);
            Gui.DefaultBackgroundTexture = GuiTextures.OptionsBackground;
            Gui.BackgroundRepeatMode = TextureRepeatMode.Tile;
        }
    }
}
