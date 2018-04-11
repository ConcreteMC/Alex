using System;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.UI.Common;

namespace Alex.Graphics.Gui.Elements.Controls
{
    public class GuiBeaconButton : GuiControl
    {

        public string Text { get; set; }

        protected GuiTextElement TextElement { get; set; }

        public GuiBeaconButton(string text, Action action)
        {
            TextElement = new GuiTextElement()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = text
            };
            AddChild(TextElement);
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
        }
    }
}
