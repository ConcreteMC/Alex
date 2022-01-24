using MiNET.Utils;
using MiNET.Utils.Nbt;

namespace Alex.Utils.Inventories.Models
{
	public struct ItemInstance
	{
		/// <summary>
		/// StackNetworkID is the network ID of the item stack. If the stack is empty, 0 is always written for this
		/// field. If not, the field should be set to 1 if the server authoritative inventories are disabled in the
		/// StartGame packet, or to a unique stack ID if it is enabled.
		/// </summary>
		public int StackNetworkId { get; set; }

		/// <summary>
		///  Stack is the actual item stack of the item instance.
		/// </summary>
		public ItemStack Stack { get; set; }

		public ItemInstance(int networkId, ItemStack stack)
		{
			StackNetworkId = networkId;
			Stack = stack;
		}
	}

	/// <summary>
	/// ItemStack represents an item instance/stack over network. It has a network ID and a metadata value that
	/// define its type.
	/// </summary>
	public struct ItemStack
	{
		public ItemType Type { get; set; }
		public short Count { get; set; }
		public Nbt NbtData { get; set; }
		public string[] CanBePlacedOn { get; set; }
		public string[] CanBreak { get; set; }

		public ItemStack(ItemType type, short count, Nbt nbt, string[] placeOn, string[] canBreak)
		{
			Type = type;
			Count = count;
			NbtData = nbt;
			CanBePlacedOn = placeOn;
			CanBreak = canBreak;
		}
	}

	/// <summary>
	///		ItemType represents a consistent combination of network ID and metadata value of an item. It cannot usually
	/// be changed unless a new item is obtained.
	/// </summary>
	public struct ItemType
	{
		public int NetworkId { get; set; }
		public short MetadataValue { get; set; }

		public ItemType(int networkId, short metadataValue)
		{
			NetworkId = networkId;
			MetadataValue = metadataValue;
		}
	}

	/// <summary>
	/// ItemEntry is an item sent in the StartGame item table. It holds a name and a legacy ID, which is used to
	/// point back to that name.
	/// </summary>
	public struct ItemEntry
	{
		public string Name { get; set; }
		public short RuntimeId { get; set; }
		public bool ComponentBased { get; set; }

		public ItemEntry(string name, short runtimeId, bool componentBased)
		{
			Name = name;
			RuntimeId = runtimeId;
			ComponentBased = componentBased;
		}
	}
}