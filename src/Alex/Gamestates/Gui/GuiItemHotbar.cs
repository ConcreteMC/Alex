using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.Graphics.Gui;
using Alex.Graphics.Gui.Rendering;

namespace Alex.Gamestates.Gui
{
    public class GuiItemHotbar : GuiElementGroup
    {
        private const int ItemWidth = 22;

        public GuiItemHotbar()
        {
            Width = 180;
            Height = 20;

            // Width = GuiScalar.FromAbsolute(180);
            //Height = GuiScalar.FromAbsolute(20);
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            Background = renderer.GetTexture(GuiTextures.Inventory_HotBar);

            for (int i = 0; i < 9; i++)
            {
                AddChild(new GuiInventoryItem()
                {
                    X = i * ItemWidth,
                    IsSelected = i == 0
                });
            }
        }
    }
}
