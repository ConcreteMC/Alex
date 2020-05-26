using System;
using System.Collections.Concurrent;
using Alex.API.Gui;
using Alex.Gui.Dialogs.Containers;
using Alex.Utils;
using Alex.Utils.Inventories;
using GLib;

namespace Alex.Worlds
{
	public class ContainerManager
	{
		private ConcurrentDictionary<byte, GuiInventoryBase> Containers { get; }
		private GuiManager GuiManager { get; }
		public ContainerManager(GuiManager guiManager)
		{
			GuiManager = guiManager;
			Containers = new ConcurrentDictionary<byte, GuiInventoryBase>();
		}

		public GuiInventoryBase Show(Inventory playerInventory, byte containerId, ContainerType type)
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
							inv = new GuiChestDialog(new InventoryBase(90), playerInventory);

							break;
						default:
							throw new NotImplementedException();
					}

					inv.Inventory.InventoryId = containerId;

					inv.OnContainerClose += (sender, args) =>
					{
						Containers.TryRemove(b, out _);
					};
					
					return inv;
				});

			GuiManager.ShowDialog(dialog);
			
			return dialog;
		}


		public bool TryGet(byte containerId, out GuiInventoryBase container)
		{
			return Containers.TryGetValue(containerId, out container);
		}
	}

	public enum ContainerType : byte
	{
		Chest = 0,
		
		Furnace
	}
}