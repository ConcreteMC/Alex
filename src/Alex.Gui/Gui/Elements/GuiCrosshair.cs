using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.UI.Common;

namespace Alex.Graphics.Gui.Elements
{
    public class GuiCrosshair : GuiElement
    {
        public GuiCrosshair()
        {
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            Width = 15;
            Height = 15;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            Background = renderer.GetTexture(GuiTextures.Crosshair);
        }
    }
}
