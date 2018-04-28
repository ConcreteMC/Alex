using Alex.API.Data;
using Alex.API.Graphics.Textures;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Items;
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

	        AddChild(_counTextElement = new GuiTextElement()
	        {
		        TextColor = TextColor.White,
		        Anchor = Alignment.BottomRight,
		        Text = "-1",
				Scale = 0.5f,
				Margin = new Thickness(0, 0, 5, 0)
	        });
		}

        protected override void OnInit(IGuiRenderer renderer)
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
            Background = IsSelected ? SelectedBackground : null;
        }

	    private GuiTextElement _counTextElement;
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
