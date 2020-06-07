using System;
using System.Collections.Concurrent;
using Alex.API.Gui;
using Alex.Gui.Dialogs.Containers;
using Alex.Utils;
using Alex.Utils.Inventories;

namespace Alex.Worlds
{
	public class InventoryManager
	{
		private ConcurrentDictionary<int, GuiInventoryBase> Containers { get; }
		private GuiManager GuiManager { get; }
		public GuiInventoryBase ActiveWindow { get; private set; }

		public InventoryManager(GuiManager guiManager)
		{
			GuiManager = guiManager;
			Containers = new ConcurrentDictionary<int, GuiInventoryBase>();
		}

		public GuiInventoryBase Show(Inventory playerInventory, int containerId, ContainerType type)
		{
			var dialog = Containers.GetOrAdd(
				containerId, b =>
				{
					GuiInventoryBase inv;
					switch (type)
					{
					//	case ContainerType.DoubleChest:
					//		inv = new GuiChestDialog(new InventoryBase(90));
//
					//		break;
						case ContainerType.Chest:
							inv = new GuiChestDialog(new InventoryBase(90)
							{
								InventoryId = containerId
							}, playerInventory);

							break;
						case ContainerType.Furnace:
							inv = new GuiFurnaceDialog(playerInventory, new InventoryBase(3)
							{
								InventoryId = containerId
							});
							break;
						default:
							throw new NotImplementedException();
					}

					inv.Inventory.InventoryId = containerId;

					inv.OnContainerClose += (sender, args) =>
					{
						Containers.TryRemove(b, out _);
						
						if (ActiveWindow == sender)
							ActiveWindow = null;
					};
					
					return inv;
				});

			GuiManager.ShowDialog(dialog);
			ActiveWindow = dialog;
			
			return dialog;
		}


		public bool TryGet(int containerId, out GuiInventoryBase container)
		{
			return Containers.TryGetValue(containerId, out container);
		}

		public void Close(int containerId)
		{
			if (Containers.TryRemove(containerId, out var container))
			{
				GuiManager.HideDialog(container);
			}
		}
	}

	public enum ContainerType : byte
	{
		Chest = 0,
		Furnace = 2,
		EnchantingTable = 3,
	}
}