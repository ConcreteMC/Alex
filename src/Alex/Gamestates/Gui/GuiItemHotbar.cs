using System.Linq;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.Gamestates.Gui
{
    public class GuiItemHotbar : GuiContainer
    {
        private const int ItemCount = 9;
        private const int ItemWidth = 20;
        
        private int _selectedIndex = 0;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= 9)
                {
                    value = 0;
                }

                if (value < 0)
                {
                    value = 8;
                }

                _selectedIndex = value;
                OnSelectedIndexChanged();
            }
        }

        public GuiItemHotbar()
        {
            Width = ItemWidth * ItemCount;
            Height = ItemWidth;
        }

        private void OnSelectedIndexChanged()
        {
            var items = Children.OfType<GuiInventoryItem>().ToArray();
            foreach (var guiInventoryItem in items)
            {
                guiInventoryItem.IsSelected = false;
            }

            items[SelectedIndex].IsSelected = true;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            Background = renderer.GetTexture(GuiTextures.Inventory_HotBar);

            for (int i = 0; i < 9; i++)
            {
                AddChild(new GuiInventoryItem()
                {
                    X = i * ItemWidth,
                    IsSelected = i == SelectedIndex
                });
            }
        }
    }
}
