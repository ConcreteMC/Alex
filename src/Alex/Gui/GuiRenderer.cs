using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Localization;
using Alex.API.Utils;
using Alex.ResourcePackLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Elements.Controls;
using RocketUI.Graphics.Textures;
using RocketUI.Utilities;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.Gui
{
    public class GuiRenderer : IGuiRenderer
    {

        private Alex Alex { get; }
        
        public GraphicsDevice GraphicsDevice { get; }
        
        public GuiScaledResolution ScaledResolution { get; set; }
        
        public GuiRenderer(Alex alex)
        {
            Alex = alex;
        }

        public Vector2 Project(Vector2 point)
        {
            return Vector2.Transform(point, ScaledResolution.TransformMatrix);
        }

        public Vector2 Unproject(Vector2 screen)
        {
            return Vector2.Transform(screen, ScaledResolution.InverseTransformMatrix);
        }
    }
}
