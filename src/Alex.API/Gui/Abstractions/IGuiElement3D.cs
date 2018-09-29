using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui
{
    public interface IGuiElement3D : IGuiElement
    {

        void Draw3D(GuiRenderArgs renderArgs);
    }
}
