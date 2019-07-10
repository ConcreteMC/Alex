using System.Linq;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gui.Elements.Inventory
{
    public class GuiItemHotbar : GuiContainer
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GuiItemHotbar));

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

		public Utils.Inventory Inventory { get; set; }

	    private GuiTextElement _itemNameTextElement;
        public GuiItemHotbar(Utils.Inventory inventory)
        {
	        Inventory = inventory;
			Inventory.SlotChanged += SlotChanged;
			Inventory.SelectedHotbarSlotChanged += SelectedHotbarSlotChanged;

            Width = ItemWidth * ItemCount;
            Height = ItemWidth;

            var hotbarItems = Inventory.GetHotbar();

            for (int i = 0; i < 9; i++)
	        {
		        AddChild(new GuiInventoryItem()
		        {
					Width = ItemWidth,
					Height = ItemWidth,
			        Margin = new Thickness((i * ItemWidth), 0, 0, 0),
					HighlightedBackground = GuiTextures.Inventory_HotBar_SelectedItemOverlay,
			        IsSelected = i == SelectedIndex,
			        Anchor = Alignment.TopLeft,
			        Item = hotbarItems[i]
		        });
	        }

	       _itemNameTextElement = new GuiTextElement()
	        {
		        Anchor = Alignment.TopCenter,
		        TextColor = TextColor.White,
		        Text = "",
		        Margin = new Thickness(0, -5, 0, 5),
				FontStyle = FontStyle.DropShadow
	        };
        }

	    private void SelectedHotbarSlotChanged(object sender, SelectedSlotChangedEventArgs e)
	    {
		    if (e.NewValue - SelectedIndex != 0)
		    {
			    SelectedIndex = e.NewValue;
		    }
	    }

	    private void SlotChanged(object sender, SlotChangedEventArgs e)
		{
			var items = Children.OfType<GuiInventoryItem>().ToArray();

			if ((Inventory.IsPeInventory && e.Index >= 0 && e.Index <= 8) || (!Inventory.IsPeInventory && e.Index >= 36 && e.Index <= 44)) //Hotbar
		    {
			    int childIndex = 8 - (44 - e.Index);
                if (Inventory.IsPeInventory)
                {
                    childIndex = e.Index;
                }
			    else if (childIndex < 0 || childIndex >= items.Length)
			    {
				    Log.Warn($"Index out of range for hotbar: {childIndex}");
					return;
			    }

			    items[childIndex].Item = e.Value;
			    if (e.Value != null)
			    {
				    items[childIndex].Name = e.Value.DisplayName;
				  /*  if (ItemFactory.TryGetItem(itemName, out Item item))
				    {
					    items[childIndex].Name = item.DisplayName;
				    }
				    else
				    {
					    items[childIndex].Name = itemName;
				    }*/
			    }

                if (childIndex == SelectedIndex)
			    {
				    OnSelectedIndexChanged();
			    }
		    }
	    }

	    private void OnSelectedIndexChanged()
        {
            var items = Children.OfType<GuiInventoryItem>().ToArray();
            foreach (var guiInventoryItem in items)
            {
                guiInventoryItem.IsSelected = false;
            }

	        var item = items[SelectedIndex];
	        item.IsSelected = true;
			
	        if (item.Item != null && !(item.Item is ItemAir))
	        {
		        if (!string.IsNullOrWhiteSpace(item.Name))
		        {
			        _itemNameTextElement.Text = item.Name;
                }
		        else
		        {
			        //_itemNameTextElement.Text = $"{item.Item.ItemID}:{item.Item.ItemDamage}";
		        }
	        }
	        else
	        {
		        _itemNameTextElement.Text = "";
	        }
        }

	    protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	    {
		    Vector2 textSize =
			    graphics.Font.MeasureString(_itemNameTextElement.Text, _itemNameTextElement.Scale);
			graphics.DrawString(Bounds.TopCenter() + new Vector2(-textSize.X / 2f, -10), _itemNameTextElement.Text, _itemNameTextElement.TextColor, _itemNameTextElement.FontStyle, _itemNameTextElement.Scale);
		    base.OnDraw(graphics, gameTime);
	    }

	    protected override void OnInit(IGuiRenderer renderer)
        {
            Background = renderer.GetTexture(GuiTextures.Inventory_HotBar);
	       
        }

	    ~GuiItemHotbar()
	    {

	    }
    }
}
