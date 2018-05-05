using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI.Elements;
using RocketUI.Elements.Controls;
using RocketUI.Graphics.Textures;
using RocketUI.Utilities;

namespace RocketUI
{
    public interface IGuiRenderer
    {
        GuiScaledResolution ScaledResolution { get; set; }
        
        Vector2 Project(Vector2 point);
        Vector2 Unproject(Vector2 screen);
    }
}
