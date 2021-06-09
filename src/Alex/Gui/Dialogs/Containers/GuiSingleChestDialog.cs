using Alex.Common.Gui.Graphics;
using Alex.Gui.Elements.Inventory;
using Alex.Items;
using Alex.Utils;
using Alex.Utils.Inventories;

namespace Alex.Gui.Dialogs.Containers
{
	public class GuiChestDialog : GuiInventoryBase
	{
		public GuiChestDialog(InventoryBase inventory, Inventory playerInventory) : base(inventory, AlexGuiTextures.InventoryChestBackground, 175, 221)
		{
			//7, 17
			
			//Add chest slots
			foreach (var slot in AddSlots(8, 18, 9, 54, 0, inventory.InventoryId))
			{
				slot.Item = inventory[slot.InventoryIndex];
			}

			//7 139
			foreach (var slot in AddSlots(8, 140, 9, 27, playerInventory.InventoryOffset, playerInventory.InventoryId))
			{
				slot.Item = playerInventory[slot.InventoryIndex];
			}

			//7, 197
			foreach (var slot in AddSlots(8, 198, 9, 9, playerInventory.HotbarOffset, playerInventory.InventoryId))
			{
				slot.Item = playerInventory[slot.InventoryIndex];
			}
		}
	}
}