using Alex.API.Data;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Alex.Items;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Graphics;
using RocketUI.Graphics.Textures;

namespace Alex.Gui.Elements.Inventory
{
    public class GuiInventoryItem : VisualElement
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnSelectedChanged(); }
        }
        
        public GuiTexture2D SelectedBackground { get;private set; }

	    private SlotData _item = new SlotData()
	    {
		    Count = 0,
		    ItemID = -1,
		    ItemDamage = 0,
		    Nbt = null
	    };

	    public SlotData Item
	    {
		    get { return _item; }
		    set
		    {
			    _item = value;
			    SlotChanged(value);
		    }
	    }

	    public GuiInventoryItem()
        {
            Height = 20;
            Width = 20;

	        AddChild(_counTextElement = new GuiMCTextElement()
	        {
		        TextColor = TextColor.White,
		        Anchor = Anchor.BottomRight,
		        Text = "-1",
				Scale = 0.75f,
				Margin = new Thickness(0, 0, 5, 3)
	        });
		}

        protected override void OnInit()
        {
            SelectedBackground = renderer.GetTexture(GuiTextures.Inventory_HotBar_SelectedItemOverlay);
	        _counTextElement.Font = renderer.Font;
        }

	    private void SlotChanged(SlotData newValue)
	    {
		    if (_counTextElement != null)
		    {
			    _counTextElement.Text = newValue.Count.ToString();
		    }
	    }

        private void OnSelectedChanged()
        {
            Background = IsSelected ? SelectedBackground : new GuiTexture2D();
        }

	    private GuiMCTextElement _counTextElement;
        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
			base.OnDraw(graphics, gameTime);
			
	        if (Item != null && Item.ItemID > 0)
	        {

	        }

            if (IsSelected)
            {
                var bounds = RenderBounds;
                bounds.Inflate(1, 1);
                graphics.FillRectangle(bounds, SelectedBackground, TextureRepeatMode.NoRepeat);
            }
        }
    }
}
