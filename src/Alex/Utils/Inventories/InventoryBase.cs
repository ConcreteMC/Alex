using System;
using Alex.Items;

namespace Alex.Utils.Inventories
{
	public class InventoryBase
	{
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
			SlotChanged?.Invoke(this, new SlotChangedEventArgs(index, value, oldValue, isServerTransaction));
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
	}
}