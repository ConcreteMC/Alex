using System;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Alex.Utils;
using NLog;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Elements.Layout;

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

	    private GuiMCTextElement _itemNameTextElement;
        public GuiItemHotbar(Utils.Inventory inventory)
        {
	        Inventory = inventory;
			Inventory.SlotChanged += SlotChanged;

            Width = ItemWidth * ItemCount;
            Height = ItemWidth;

	        for (int i = 0; i < 9; i++)
	        {
		        AddChild(new GuiInventoryItem()
		        {
			        Margin = new Thickness((i * ItemWidth), 0, 0, 0),
			        IsSelected = i == SelectedIndex,
			        Anchor = Anchor.TopLeft,
			        Item = Inventory[36 + i]
		        });
	        }

	        AddChild(_itemNameTextElement = new GuiMCTextElement()
	        {
		        Anchor = Anchor.TopCenter,
		        TextColor = TextColor.White,
		        Text = "Unknown",
		        Margin = new Thickness(0, -5, 0, 5)
	        });
		}

	    private void SlotChanged(object sender, SlotChangedEventArgs e)
		{
			var items = Children.OfType<GuiInventoryItem>().ToArray();

			if (e.Index >= 36 && e.Index <= 44) //Hotbar
		    {
			    int childIndex = 8 - (44 - e.Index);
			    if (childIndex < 0 || childIndex >= items.Length)
			    {
				    Log.Warn($"Index out of range for hotbar: {childIndex}");
					return;
			    }

			    items[childIndex].Item = e.Value;

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

	        if (ItemFactory.ResolveItemName(item.Item.ItemID, out string itemName))
	        {
		        _itemNameTextElement.Text = itemName;

	        }
        }

        protected override void OnInit()
		{
			Background = GuiTextures.Inventory_HotBar;

			AddChild(_itemNameTextElement = new GuiMCTextElement()
			{
				Anchor = Anchor.TopCenter,
				TextColor = TextColor.White,
				Text = "Unknown",
				Margin = new Thickness(0, -5, 0, 5)
			});
        }
    }
}
