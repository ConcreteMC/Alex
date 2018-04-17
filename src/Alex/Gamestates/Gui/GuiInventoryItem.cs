using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
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

        public TextureSlice2D SelectedBackground { get;private set; }

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
                var bounds = RenderBounds;
                bounds.Inflate(1, 1);
                args.Draw(SelectedBackground, bounds, TextureRepeatMode.NoRepeat);
            }
        }
    }
}
