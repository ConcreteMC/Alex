using Alex.API.Graphics.Textures;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Elements.Inventory
{
    public class GuiInventoryItem : GuiElement
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnSelectedChanged(); }
        }
        
        public TextureSlice2D SelectedBackground { get;private set; }

        public GuiInventoryItem()
        {
            Height = 20;
            Width = 20;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            SelectedBackground = renderer.GetTexture(GuiTextures.Inventory_HotBar_SelectedItemOverlay);
        }

        private void OnSelectedChanged()
        {
            Background = IsSelected ? SelectedBackground : null;
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            if (IsSelected)
            {
                var bounds = RenderBounds;
                bounds.Inflate(1, 1);
                graphics.FillRectangle(bounds, SelectedBackground, TextureRepeatMode.NoRepeat);
            }
        }
    }
}
