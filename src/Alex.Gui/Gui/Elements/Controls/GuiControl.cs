using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.Graphics.Gui.Elements.Controls
{
    public class GuiControl : GuiContainer
    {

        public GuiControlState ControlState { get; set; } = GuiControlState.Default;
    }
}
