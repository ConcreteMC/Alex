using System.Linq;
using Alex.Common.Gui.Graphics;
using Alex.Common.Utils;
using Alex.Items;
using Alex.Utils;
using Alex.Utils.Inventories;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;
using RocketUI.Utilities.Extensions;


namespace Alex.Gui.Elements.Inventory
{
    public class GuiItemHotbar : Container
    {
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(GuiItemHotbar));

		private const int ItemCount = 9;
        private const int ItemWidth = 18;
        
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

        private Utils.Inventories.Inventory _inventory = null;

        public Utils.Inventories.Inventory Inventory
        {
	        get
	        {
		        return _inventory;
	        }
	        set
	        {
		        var oldValue = _inventory;

		        if (oldValue != null)
		        {
			        oldValue.SlotChanged -= SlotChanged;
			        oldValue.SelectedHotbarSlotChanged -= SelectedHotbarSlotChanged;
		        }

		        value.SlotChanged += SlotChanged;
		        value.SelectedHotbarSlotChanged += SelectedHotbarSlotChanged;
		        
		        _inventory = value;
		        
		        var hotbarItems = value.GetHotbar();
		        for (int i = 0; i < 9; i++)
		        {
			        _hotbarItems[i].Item = hotbarItems[i];
		        }
	        }
        }

        private Container _hotbar;
	    private FadingTextElement _itemNameTextElement;
	    private GuiInventoryItem[] _hotbarItems;
        public GuiItemHotbar(Utils.Inventories.Inventory inventory)
        {
	        _hotbarItems = new GuiInventoryItem[9];
	        _hotbar = new Container()
	        {
		        Anchor = Alignment.Fill,
		        //Padding = new Thickness(4,4)
	        };
            
	        AddChild(_hotbar);
	        for (int i = 0; i < 9; i++)
	        {
		        _hotbar.AddChild(_hotbarItems[i] = new GuiInventoryItem()
		        {
			        Width = ItemWidth + 4,
			        Height = ItemWidth + 4,
			        //Padding = new Thickness(2, 2),
			        Margin = new Thickness( (i * (ItemWidth + 4)), 0, 4, 0),
			        HighlightedBackground = AlexGuiTextures.Inventory_HotBar_SelectedItemOverlay,
			        IsSelected = i == SelectedIndex,
			        Anchor = Alignment.TopLeft,
			        //  Item = hotbarItems[i],
			        AutoSizeMode = AutoSizeMode.None,
			        CanHighlight = false
		        });
	        }
	        
	        Inventory = inventory;
			//Inventory.SlotChanged += SlotChanged;
			//Inventory.SelectedHotbarSlotChanged += SelectedHotbarSlotChanged;

            Width = (ItemWidth + 4) * ItemCount;
            Height = ItemWidth + 4;

            AutoSizeMode = AutoSizeMode.None;

            _itemNameTextElement = new FadingTextElement()
	        {
		        Anchor = Alignment.TopCenter,
		        TextColor = (Color) TextColor.White,
		        Text = "",
		        Margin = new Thickness(0, -5, 0, 5),
				FontStyle = FontStyle.DropShadow,
				IsVisible = false,
				Background = Color.Black * 0.5f
	        };
	       
	       AddChild(_itemNameTextElement);
        }

        private bool _showItemCount = true;

        public bool ShowItemCount
        {
	        get
	        {
		        return _showItemCount;
	        }
	        set
	        {
		        _showItemCount = value;
		        
		        var items = ChildElements.OfType<GuiInventoryItem>().ToArray();

		        foreach (var item in items)
		        {
			        item.ShowCount = value;
		        }
	        }
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
			var items = _hotbarItems;

			bool isBedrock = Inventory is BedrockInventory;
			if ((isBedrock && e.Index >= 0 && e.Index <= 8) || (!isBedrock && e.Index >= 36 && e.Index <= 44)) //Hotbar
		    {
			    int childIndex = 8 - (44 - e.Index);
                if (isBedrock)
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
				    //items[childIndex].Name = e.Value.GetDisplayName();
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
            var items = _hotbarItems;
            foreach (var guiInventoryItem in items)
            {
                guiInventoryItem.IsSelected = false;
            }

	        var item = items[SelectedIndex];
	        item.IsSelected = true;
			
	        if (item.Item != null && !item.Item.IsAir())
	        {
		        var displayName = item.Item?.GetDisplayName();
		        if (!string.IsNullOrWhiteSpace(displayName))
		        {
			        _itemNameTextElement.Text = displayName;
                }
		        else
		        {
			        //_itemNameTextElement.Text = $"{item.Item.ItemID}:{item.Item.ItemDamage}";
		        }
	        }
	        else
	        {
		        _itemNameTextElement.Text = string.Empty;
	        }
        }

	    protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
	    {
		    base.OnDraw(graphics, gameTime);
		    var opacity = _itemNameTextElement.TextOpacity;

		    if (opacity > 0.1f)
		    {
			    var textSize = graphics.Font.MeasureString(_itemNameTextElement.Text, _itemNameTextElement.Scale).ToPoint();

			    var textPosition = Bounds.TopCenter() - new Vector2((textSize.X / 2f), (textSize.Y + 2));
			    var rectanglePosition = textPosition.ToPoint();
			    
			    var rect = new Rectangle(rectanglePosition.X - 2, rectanglePosition.Y - 2, textSize.X + 4, textSize.Y + 2);

			    graphics.FillRectangle(rect, Color.Black * (0.75f) * opacity);
			    graphics.DrawString(
				    textPosition, _itemNameTextElement.Text, _itemNameTextElement.TextColor, _itemNameTextElement.FontStyle,
				    _itemNameTextElement.Scale, opacity: opacity);
		    }
	    }

	    protected override void OnInit(IGuiRenderer renderer)
        {
            _hotbar.Background = renderer.GetTexture(AlexGuiTextures.Inventory_HotBar);
	       
        }

	    ~GuiItemHotbar()
	    {

	    }
    }
}
