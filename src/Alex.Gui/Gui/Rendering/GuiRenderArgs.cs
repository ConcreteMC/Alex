using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui.Rendering
{
    public class GuiRenderArgs
    {
        public IGuiRenderer Renderer { get; }

        public GraphicsDevice Graphics { get; }
        public SpriteBatch SpriteBatch { get; }

        public GuiRenderArgs(IGuiRenderer renderer, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            Renderer = renderer;
            Graphics = graphicsDevice;
            SpriteBatch = spriteBatch;
        }
    }
}
