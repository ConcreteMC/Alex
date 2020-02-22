using System;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

namespace Alex.Gui.Elements.Inventory
{
	public class GuiInventoryItem : GuiControl
	{
		private bool _isSelected;

		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; OnSelectedChanged(); }
		}

		private string _name = String.Empty;
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				//NameChanged(value);
			}
		}

		public TextureSlice2D SelectedBackground { get; private set; }
		private GuiTextureElement Texture { get; set; }

		private Item _item = new ItemAir()
		{
			Count = 0,
			//  ItemID = -1,
			//   ItemDamage = 0,
			Nbt = null
		};

		public Item Item
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
			Height = 18;
			Width = 18;

			AddChild(Texture = new GuiTextureElement()
			{
				Anchor = Alignment.TopLeft,

				Height = 16,
				Width = 16,
				Margin = new Thickness(4, 4)
			});

			AddChild(_counTextElement = new GuiTextElement()
			{
				TextColor = TextColor.White,
				Anchor = Alignment.BottomRight,
				Text = "",
				Scale = 0.75f,
				Margin = new Thickness(0, 0, 5, 3)
			});
		}

		protected override void OnInit(IGuiRenderer renderer)
		{
			SelectedBackground = renderer.GetTexture(GuiTextures.Inventory_HotBar_SelectedItemOverlay);
			_counTextElement.Font = renderer.Font;
		}

		private void NameChanged(string newName)
		{
			if (!string.IsNullOrWhiteSpace(newName))
			{
				if (ItemFactory.ResolveItemTexture(newName, out Texture2D texture))
				{

					Texture.Texture = texture;
				}
				else
				{

				}
			}
			else
			{
				Texture.Texture = null;
			}
		}

		private void SlotChanged(Item newValue)
		{
			if (newValue != null)
			{
				NameChanged(newValue.Name);
			}
			
			if (_counTextElement != null)
			{
				if (newValue != null && newValue.Count != 0)
				{
					_counTextElement.Text = newValue.Count.ToString();
				}
				else
				{
					_counTextElement.Text = "";
				}
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

			// if (Item != null && Item.ItemID > 0)
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
