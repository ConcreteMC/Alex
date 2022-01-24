using Alex.Items;
using RocketUI.Input;

namespace Alex.Utils.Inventories
{
	public interface IInventoryTransactionHandler
	{
		void DialogClosed();

		void SlotClicked(MouseButton button, byte inventoryId, byte slotId);

		void SlotHover(byte inventoryId, byte slotId);
	}
}