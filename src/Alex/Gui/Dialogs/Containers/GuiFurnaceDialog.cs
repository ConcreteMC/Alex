using Alex.API.Gui.Graphics;
using Alex.Utils;
using Alex.Utils.Inventories;

namespace Alex.Gui.Dialogs.Containers
{
	public class GuiFurnaceDialog : GuiInventoryBase
	{
		private Inventory PlayerInventory { get; set; }
		/// <inheritdoc />
		public GuiFurnaceDialog(Inventory playerInventory, InventoryBase inventory) : base(inventory, GuiTextures.InventoryFurnace, 175, 165)
		{
			PlayerInventory = playerInventory;

			var topSlot = AddSlot(56, 17, 0, inventory.InventoryId);
			var bottomSlot = AddSlot(56, 53, 1, inventory.InventoryId);
			
			var outputSlot = AddSlot(112, 31, 2, inventory.InventoryId);
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