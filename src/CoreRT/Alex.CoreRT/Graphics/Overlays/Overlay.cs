using Alex.CoreRT.Gamestates;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.CoreRT.Graphics.Overlays
{
    public interface IOverlay
    {
	    void Load(GraphicsDevice graphics, ResourceManager resources);
	    void Render(RenderArgs args);
    }
}
