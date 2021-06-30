using System.Linq;
using Alex.Blocks;
using Alex.Items;
using Alex.Worlds.Chunks;
using Alex.Worlds.Multiplayer.Bedrock;
using Alex.Worlds.Singleplayer;
using MiNET.Net;
using MiNET.Utils;
using Newtonsoft.Json;
using NLog;

namespace Alex.Utils.Inventories
{
	public class ItemStackInventory : BedrockInventory
	{
		private BedrockClient Client { get; }
		//private readonly ItemWrapper[] _slots;

		/// <inheritdoc />
		public ItemStackInventory(BedrockClient bedrockClient) : base(46)
		{
			Client = bedrockClient;
		}

		//public MiNET.Items.Item GetOriginal(int slot)
		//{
		//	return Slots[slot];
		//}
		
		/// <inheritdoc />
		protected override Item Get(int index)
		{
			return Slots[index];
		}

		/// <inheritdoc />
		protected override void Set(int index, Item value, bool isClientTransaction)
		{
			
		}

		private void Set(int index, Item wrapper)
		{
			var oldValue = Slots[index];
			Slots[index] = wrapper;
			
			InvokeSlotChanged(new SlotChangedEventArgs(InventoryId, index, wrapper, oldValue, true));
		}
		
		public void HandleInventoryContent(uint inventoryId, ItemStacks stacks)
		{
			if (inventoryId == 0x00)
			{
				for (var index = 0; index < stacks.Count; index++)
				{
					var itemStack = stacks[index];
					Set( index, itemStack.ToAlexItem());
					//base.SetSlot();
				}
			}
			else if (inventoryId == 0x7c) //UI Slots
			{
				
			}
		}

		public void HandleSetSlot(McpeInventorySlot packet)
		{
			if (packet.inventoryId == 0x00)
			{
				Set((int) packet.slot, packet.item.ToAlexItem());
				//_slots[packet.slot] = wrapper;
				return;
			}
		}
		
		public void HandleResponses(ItemStackResponses responses)
		{
			
		}
		
		protected void SetContainerItem(int containerId, int slot, Item item)
		{
			//if (UsingAnvil && containerId < 3) containerId = 13;

			switch (containerId)
			{
				case 13: // crafting
				case 21: // enchanting
				case 22: // enchanting
				case 41: // loom
				case 58: // cursor
				case 59: // creative
					//UiInventory.Slots[slot] = item;
					break;
				case 12: // auto
				case 27: // hotbar
				case 28: // player inventory
					//Slots[slot] = item;
					break;
				case 6: // armor
					switch (slot)
					{
						case 0:
							Helmet = item;
							break;
						case 1:
							Chestplate = item;
							break;
						case 2:
							Leggings = item;
							break;
						case 3:
							Boots = item;
							break;
					}
					break;
				case 7: // chest/container
					//if (_openInventory is Inventory inventory) inventory.SetSlot(this, (byte) slot, item);
					break;
				default:
			//		Log.Warn($"Unknown containerId: {containerId}");
					break;
			}
		}
	}

	public static class ItemsExtensions
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
		
		private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};

		public static Item ToAlexItem(this MiNET.Items.Item item)
		{
			if (item == null)
				return new ItemAir();

			Item result = null;


			var itemState = ChunkProcessor.Itemstates.FirstOrDefault(x => x.Id == item.Id);

			if (itemState == null)
				itemState = MiNET.Items.ItemFactory.Itemstates.FirstOrDefault(x => x.Id == item.Id);

			if (itemState != null)
			{
				if (ItemFactory.TryGetItem(itemState.Name, out result))
				{
					//	Log.Info($"{item.Id} = {JsonConvert.SerializeObject(itemState, SerializerSettings)} || {JsonConvert.SerializeObject(result, SerializerSettings)}");
				}
			}

			if (result == null && item.Id < 256 && item.Id >= 0) //Block
			{
				var id = item.Id;
				var meta = (byte) item.Metadata;

				var reverseMap = MiNET.Worlds.AnvilWorldProvider.Convert.FirstOrDefault(map => map.Value.Item1 == id);

				if (reverseMap.Value != null)
				{
					id = (byte) reverseMap.Key;
				}

				var res = BlockFactory.GetBlockStateID(id, meta);

				if (AnvilWorldProvider.BlockStateMapper.TryGetValue(res, out var res2))
				{
					var t = BlockFactory.GetBlockState(res2);

					ItemFactory.TryGetItem(t.Name, out result);
				}
			}

			if (result != null)
			{
				result.StackID = item.UniqueId;
				result.Meta = item.Metadata;
				result.Count = item.Count;
				result.Nbt = item.ExtraData;
				result.Id = item.Id;

				return result;
			}

			return new ItemAir() {Count = 0};
		}
	}
}