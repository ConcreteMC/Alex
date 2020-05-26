using Alex.API.Gui.Graphics;
using Alex.Utils;
using Alex.Utils.Inventories;
using RocketUI;

namespace Alex.Gui.Dialogs.Containers
{
	public class GuiChestDialog : GuiInventoryBase
	{
		public GuiChestDialog(InventoryBase inventory, Inventory playerInventory) : base(inventory)
		{
			ContentContainer.Background = GuiTextures.InventoryChestBackground;
			ContentContainer.BackgroundOverlay = null;
			
			ContentContainer.Width = ContentContainer.MinWidth = ContentContainer.MaxWidth = 175;
			ContentContainer.Height = ContentContainer.MinHeight = ContentContainer.MaxHeight = 221;
            
			SetFixedSize(175, 221);

			ContentContainer.AutoSizeMode = AutoSizeMode.None;
			
			//7, 17
			
			//Add chest slots
			foreach (var slot in AddSlots(8, 18, 9, 54, 0, inventory.InventoryId))
			{
				slot.Item = inventory[slot.InventoryIndex];
			}

			//7 139
			foreach (var slot in AddSlots(8, 140, 9, 27, 9, playerInventory.InventoryId))
			{
				slot.Item = playerInventory[slot.InventoryIndex];
			}

			//7, 197
			foreach (var slot in AddSlots(8, 198, 9, 9, 0, playerInventory.InventoryId))
			{
				slot.Item = playerInventory[slot.InventoryIndex];
			}
		}
	}
}