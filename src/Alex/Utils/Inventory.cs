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

	    public Inventory(int slots)
	    {
			Slots = new SlotData[slots];
			Empty();
	    }

	    public void Empty()
	    {
		    for (int i = 0; i < Slots.Length; i++)
		    {
			    Slots[i] = new SlotData();
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
		    }
	    }
    }
}
