using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Alex.API.Data;
using Alex.Blocks;
using Alex.Items;

namespace Alex.Utils
{
    public class Inventory
    {
		private SlotData[] Slots { get; }
	    public int SlotCount => Slots.Length;
	    public byte SelectedSlot { get; set; } = 0;

	    public EventHandler<SlotChangedEventArgs> SlotChanged = null;

	    public Inventory(int slots)
	    {
			Slots = new SlotData[slots];
			Empty();
	    }

	    public void Empty()
	    {
		    for (int i = 0; i < Slots.Length; i++)
		    {
			    Slots[i] = new SlotData()
			    {
					ItemID = -1,
					Count = 0,
					ItemDamage = 0,
					Nbt = null
			    };
		    }
		}

	    public SlotData this[int index]
	    {
		    get
		    {
				if (index < 0 || index >= Slots.Length) throw new IndexOutOfRangeException();

			    return Slots[index];
			}
		    set
		    {
			    if (index < 0 || index >= Slots.Length) throw new IndexOutOfRangeException();

			    Slots[index] = value;
			    SlotChanged?.Invoke(this, new SlotChangedEventArgs(index, value));
			}
	    }
    }

	public class SlotChangedEventArgs : EventArgs
	{
		public int Index;
		public SlotData Value;

		public SlotChangedEventArgs(int index, SlotData value)
		{
			Index = index;
			Value = value;
		}
	}
}
