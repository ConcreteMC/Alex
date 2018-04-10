using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.Gui;
using Alex.Graphics.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates.Gui
{
    public class GuiInventoryItem : GuiElement
    {
        public bool IsSelected { get; set; }

        public Texture2D SelectedBackground { get;private set; }

        public GuiInventoryItem()
        {
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            SelectedBackground = renderer.GetTexture(GuiTextures.Inventory_HotBar_SelectedItemOverlay);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (!IsSelected && Background != null)
            {
                Background = null;
            }
            else if(IsSelected && Background == null)
            {
                Background = SelectedBackground;
            }
        }
    }
}
