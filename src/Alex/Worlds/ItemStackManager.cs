using System.Collections.Concurrent;
using Alex.Entities;
using Alex.Items;
using Alex.Utils.Inventories;
using log4net;
using MiNET.Utils;

namespace Alex.Worlds
{
	public class ItemStackManager
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ItemStackManager));
		
		private                 Player       _player;
		//private ConcurrentDictionary<uint, ItemStackAction> _ac
		public ItemStackManager(Player player)
		{
			_player = player;
		}
		
		
		private Item GetContainerItem(int containerId, int slot)
		{
			//if (_player.UsingAnvil && containerId < 3) containerId = 13;

			Item item = null;
			switch (containerId)
			{
				case 13: // crafting
				case 21: // enchanting
				case 22: // enchanting
				case 41: // loom
				case 58: // cursor
				case 59: // creative
					item = _player.Inventory.UiInventory[slot];
					break;
				case 12: // auto
				case 27: // hotbar
				case 28: // player inventory
					item = _player.Inventory[slot];
					break;
				case 33: // off-hand
					item = _player.Inventory.OffHand;
					break;
				case 6: // armor
					item = slot switch
					{
						0 => _player.Inventory.Helmet,
						1 => _player.Inventory.Chestplate,
						2 => _player.Inventory.Leggings,
						3 => _player.Inventory.Boots,
						_ => null
					};
					break;
				case 7: // chest/container
					var cont = _player.Level.InventoryManager.ActiveWindow;

					if (cont?.Inventory != null)
					{
						item = cont.Inventory[slot];
					}
				//	if (_player.Level.InventoryManager.ActiveWindow?.Inventory is Inventory inventory)
					//if (_player._openInventory is Inventory inventory) item = inventory.GetSlot((byte) slot);
					break;
				default:
					Log.Warn($"Unknown containerId: {containerId}");
					break;
			}

			return item;
		}

		private void SetContainerItem(int containerId, int slot, Item item)
		{
			//if (_player.UsingAnvil && containerId < 3) containerId = 13;

			switch (containerId)
			{
				case 13: // crafting
				case 21: // enchanting
				case 22: // enchanting
				case 41: // loom
				case 58: // cursor
				case 59: // creative
					_player.Inventory.UiInventory[slot] = item;
					break;
				case 12: // auto
				case 27: // hotbar
				case 28: // player inventory
					_player.Inventory.SetSlot(slot, item, false);// .Slots[slot] = item;
					break;
				case 33: // off-hand
					_player.Inventory.OffHand = item;
					break;
				case 6: // armor
					switch (slot)
					{
						case 0:
							_player.Inventory.Helmet = item;
							break;
						case 1:
							_player.Inventory.Chestplate = item;
							break;
						case 2:
							_player.Inventory.Leggings = item;
							break;
						case 3:
							_player.Inventory.Boots = item;
							break;
					}
					break;
				case 7: // chest/container
					var cont = _player.Level.InventoryManager.ActiveWindow;

					if (cont?.Inventory != null)
					{
						cont.Inventory.SetSlot(slot, item, false);
					}
					//if (_player._openInventory is Inventory inventory) inventory.SetSlot(_player, (byte) slot, item);
					break;
				default:
					Log.Warn($"Unknown containerId: {containerId}");
					break;
			}
		}
	}
}