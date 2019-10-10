using Alex.API.Graphics.Typography;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using RocketUI;

namespace Alex.Gui.Elements.Inventory
{
    public class InventoryContainerItem : GuiControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(InventoryContainerItem));
        
        private Item _item;
        private GuiTextureElement TextureElement { get; }

        public InventoryContainerItem()
        {
            SetFixedSize(18, 18);
            Padding = new Thickness(2);

            AddChild(TextureElement = new GuiTextureElement()
            {
                Anchor = Alignment.Fill
            });
        }
        
        public Item Item
        {
            get => _item;
            set
            {
                _item = value;

                if (string.IsNullOrWhiteSpace(value?.Name))
                {
                   // if (!ItemFactory.TryGetItem())
                   Log.Warn($"Item name is null or whitespace!");
                    return;
                }

               // TextOverlay.Text = value?.DisplayName ?? value.Name;
                
                if (ItemFactory.ResolveItemTexture(_item.Name, out Texture2D texture))
                {
                    TextureElement.Texture = texture;
                }
                else
                {
                    Log.Warn($"Could not resolve item texture: {_item.Name}");
                }
            }
        }

        protected override void OnCursorPressed(Point cursorPosition)
        {
            base.OnCursorPressed(cursorPosition);
        }

        private bool _showTooltip = false;
        protected override void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
        {
            _showTooltip = true;
           // TextOverlay.RenderPosition = RenderPosition;
            
            base.OnCursorMove(cursorPosition, previousCursorPosition, isCursorDown);
        }

        protected override void OnCursorEnter(Point cursorPosition)
        {
            _showTooltip = true;
          //  AddChild(TextOverlay);
            
            base.OnCursorEnter(cursorPosition);
        }

        protected override void OnCursorLeave(Point cursorPosition)
        {
            base.OnCursorLeave(cursorPosition);
            _showTooltip = false;
            
          //  RemoveChild(TextOverlay);
        }
    }
}