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
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnSelectedChanged(); }
        }

        public override int Width => 20;
        public override int Height => 20;

        public Texture2D SelectedBackground { get;private set; }

        public GuiInventoryItem()
        {
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            SelectedBackground = renderer.GetTexture(GuiTextures.Inventory_HotBar_SelectedItemOverlay);
        }

        private void OnSelectedChanged()
        {
            Background = IsSelected ? SelectedBackground : null;
        }

        protected override void OnDraw(GuiRenderArgs args)
        {
            if (IsSelected)
            {
                var bounds = Bounds;
                bounds.Inflate(1, 1);
                args.SpriteBatch.Draw(SelectedBackground, bounds, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            }
        }
    }
}
