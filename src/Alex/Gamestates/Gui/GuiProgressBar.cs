using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.Gui;
using Alex.Graphics.Gui.Rendering;

namespace Alex.Gamestates.Gui
{
    public class GuiProgressBar : GuiElement
    {

        public int Value { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }


        protected override void OnDraw(GuiRenderArgs renderArgs)
        {
            base.OnDraw(renderArgs);
        }
    }
}
