using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics
{
    public class GraphicsManager
    {
        public GraphicsDevice GraphicsDevice { get; }

        public GraphicsManager(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
    }
}