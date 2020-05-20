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
	public class GuiInventoryItem : InventoryContainerItem
	{
		private bool _isSelected;

		public bool IsSelected
		{
			get { return _isSelected; }
			set { _isSelected = value; OnSelectedChanged(); }
		}

		public TextureSlice2D SelectedBackground { get; private set; }


		public GuiInventoryItem()
		{
			
		}

		protected override void OnInit(IGuiRenderer renderer)
		{
			SelectedBackground = renderer.GetTexture(GuiTextures.Inventory_HotBar_SelectedItemOverlay);
			//_counTextElement.Font = renderer.Font;
			base.OnInit(renderer);
		}
		
		private void OnSelectedChanged()
		{
			Background = IsSelected ? SelectedBackground : null;
		}
		
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);

			if (IsSelected)
			{
				var bounds = RenderBounds;
				bounds.Inflate(1, 1);
				graphics.FillRectangle(bounds, SelectedBackground, TextureRepeatMode.NoRepeat);
			}
		}
	}
}
