using Alex.API.Gui.Graphics;
using Alex.Utils;
using Alex.Utils.Inventories;

namespace Alex.Gui.Dialogs.Containers
{
	public class CraftingTableDialog : GuiInventoryBase
	{
		/// <inheritdoc />
		public CraftingTableDialog(InventoryBase inventory, Inventory playerInventory) : base(
			inventory, GuiTextures.InventoryCraftingTable, 175, 165)
		{
			//7, 17
			
			//Add chest slots
			foreach (var slot in AddSlots(30, 17, 3, 9, 0, inventory.InventoryId))
			{
				slot.Item = inventory[slot.InventoryIndex];
			}

			var outputSlot = AddSlot(120, 31, 10, inventory.InventoryId);
			outputSlot.Width = 24;
			outputSlot.Height = 24;
			
			//7 139
			foreach (var slot in AddSlots(8, 84, 9, 27, 9, playerInventory.InventoryId))
			{
				slot.Item = playerInventory[slot.InventoryIndex];
			}

			//7, 197
			foreach (var slot in AddSlots(8, 142, 9, 9, 0, playerInventory.InventoryId))
			{
				slot.Item = playerInventory[slot.InventoryIndex];
			}
		}
	}
}