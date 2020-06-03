using Alex.API.Gui;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Graphics;
using Alex.Gui.Elements.Inventory;
using Alex.Items;
using Alex.Utils.Inventories;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Dialogs.Containers
{
	public class GuiPlayerCreativeInventoryDialog : GuiInventoryBase
	{
		private GuiScrollableMultiStackContainer MultiStackContainer { get; set; }
		public GuiPlayerCreativeInventoryDialog(Item[] items) : base(new InventoryBase(items.Length), GuiTextures.InventoryCreativeItemSearch, 194, 135)
		{
			MultiStackContainer = new GuiScrollableMultiStackContainer()
			{
				Anchor = Alignment.Fill,
				Orientation = Orientation.Vertical,
				Margin = new Thickness(8, 17, 24, 27),
				AutoSizeMode = AutoSizeMode.None,
				Height = 90,
				MaxHeight = 90,
				MaxWidth = 162,
				Width = 162
			};
			
			ContentContainer.AddChild(MultiStackContainer);
			
			GuiStackContainer stackContainer = MultiStackContainer.AddRow(RowBuilder);
			int itemsX = 0;
			for (int i = 0; i < items.Length; i++)
			{
				var slot = CreateSlot(0, 0, i, 0);
				
				stackContainer.AddChild(slot);
				
				//x += InventoryContainerItem.ItemWidth;
				itemsX++;
				
				if (itemsX == 9)
				{
					itemsX = 0;
					stackContainer = MultiStackContainer.AddRow(RowBuilder);
				}
			}
		}

		private void RowBuilder(GuiStackContainer obj)
		{
			obj.Orientation = Orientation.Horizontal;
		}
	}
}