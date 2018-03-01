using Alex.Gamestates;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Overlays
{
    public interface IOverlay
    {
	    void Load(GraphicsDevice graphics, ResourceManager resources);
	    void Render(RenderArgs args);
    }
}
