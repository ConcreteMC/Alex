using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Alex.Gamestates;
using Microsoft.Xna.Framework;

namespace Alex.GameStates.Gui.Common
{
    public class GuiStateBase : GameState
    {

        public GuiStateBase() : base(Alex.Instance)
        {
            Gui = new GuiScreen(Alex)
            {
                DefaultBackgroundTexture = GuiTextures.OptionsBackground,
                BackgroundRepeatMode = TextureRepeatMode.Tile,
                BackgroundOverlayColor = new Color(Color.Black, 0.25f),
                BackgroundScale = new Vector2(2f, 2f)
            };
        }
    }
}
