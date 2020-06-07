using System;
using System.Collections.Generic;
using Alex.API.Input;
using Alex.Items;
using Alex.Networking.Java.Packets.Play;

namespace Alex.Utils.Inventories
{
	public class InventoryBase
	{
		public int InventoryId { get; set; } = 0;
		
		protected Item[] Slots     { get; }
		public    int    SlotCount => Slots.Length;

		public InventoryBase(int slotCount)
		{
			Slots = new Item[slotCount];
			Empty();
		}

		public event EventHandler<SlotChangedEventArgs> SlotChanged = null;
		
		public void Empty()
		{
			for (int i = 0; i < Slots.Length; i++)
			{
				Slots[i] = new ItemAir()
				{
					Count = 0
				};
			}
		}
		
		public void SetSlot(int index, Item value, bool isServerTransaction)
		{
			if (index < 0 || index >= Slots.Length) throw new IndexOutOfRangeException();
			if (value.Count == 0)
			{
				value = new ItemAir()
				{
					Count = 0
				};
			}

			var oldValue = Slots[index];
                
			Slots[index] = value;
			/*if ((index == 36 + _selectedSlot && !IsPeInventory) || (index == _selectedSlot && IsPeInventory))
			{
			    MainHand = value;
			}*/
			SlotChanged?.Invoke(this, new SlotChangedEventArgs(InventoryId, index, value, oldValue, isServerTransaction));
		}
		
		public Item this[int index]
		{
			get
			{
				if (index < 0 || index >= Slots.Length) throw new IndexOutOfRangeException();

				return Slots[index];
			}
			set
			{
				SetSlot(index, value, false);
			}
		}

		public Dictionary<short, (ClickWindowPacket packet, SlotChangedEventArgs slot, bool isCursorTransaction)>
			UnconfirmedWindowTransactions { get; } =
			new Dictionary<short, (ClickWindowPacket packet, SlotChangedEventArgs slot, bool isCursorTransaction)>();
		
		public int ActionNumber { get; set; } = 1;
		public event EventHandler<CursorChangedEventArgs> CursorChanged = null;
		public void SetCursor(Item item, bool isServerTransaction, int index = -2, MouseButton button = MouseButton.Left)
		{
			var oldValue = _cursor;
			Cursor = item;

			CursorChanged?.Invoke(this, new CursorChangedEventArgs(InventoryId, index, item, oldValue, isServerTransaction, button));
		}
		
		
		private Item _cursor;

		public virtual Item Cursor
		{
			get
			{
				return _cursor;
			}
			private set
			{
				// var oldValue = _cursor;
				_cursor = value;
				// CursorChanged?.Invoke(this, new SlotChangedEventArgs(0, value, oldValue, true));
			}
		}
	}
}