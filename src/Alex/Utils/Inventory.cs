using System;
using Alex.Items;

namespace Alex.Utils
{
    public class Inventory
    {
		private Item[] Slots { get; }
	    public int SlotCount => Slots.Length;

	    private byte _selectedSlot = 0;

	    public int SelectedSlot
	    {
		    get { return _selectedSlot; }
		    set
		    {
			    if (value >= 9)
			    {
				    value = 0;
			    }

			    if (value < 0)
			    {
				    value = 8;
			    }

				byte oldValue = _selectedSlot;
			    _selectedSlot = (byte)value;
		        MainHand = Slots[36 + value];
			    SelectedHotbarSlotChanged?.Invoke(this, new SelectedSlotChangedEventArgs(oldValue, (byte)value));
			}
	    }

        public bool IsPeInventory { get; set; } = false;

        public Item MainHand;
	    public Item OffHand;
	    public Item Helmet;
	    public Item Chestplate;
	    public Item Leggings;
	    public Item Boots;

	    public event EventHandler<SlotChangedEventArgs> SlotChanged = null;
	    public event EventHandler<SelectedSlotChangedEventArgs> SelectedHotbarSlotChanged = null;

	    public Inventory(int slots)
	    {
			Slots = new Item[slots];
			Empty();
	    }

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

        public int[] PocketHotbar = new int[9]
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8
        };
        public Item[] GetHotbar()
        {
            Item[] items = new Item[9];
            if (!IsPeInventory)
            {
                for (int i = 0; i < 9; i++)
                {
                   items[i] = this[36 + i];
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    items[i] = this[PocketHotbar[i]];
                }
            }

            return items;
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
			    if (index < 0 || index >= Slots.Length) throw new IndexOutOfRangeException();
                if (value.Count == 0)
                {
                    value = new ItemAir()
                    {
                        Count = 0
                    };
                }
			    Slots[index] = value;
		        if ((index == 36 + _selectedSlot && !IsPeInventory) || (index == _selectedSlot && IsPeInventory))
		        {
		            MainHand = value;
		        }
			    SlotChanged?.Invoke(this, new SlotChangedEventArgs(index, value));
			}
	    }
    }

	public class SlotChangedEventArgs : EventArgs
	{
		public int Index;
		public Item Value;

		public SlotChangedEventArgs(int index, Item value)
		{
			Index = index;
			Value = value;
		}
	}

	public class SelectedSlotChangedEventArgs : EventArgs
	{
		public byte PreviousValue, NewValue;
		public SelectedSlotChangedEventArgs(byte oldValue, byte newValue)
		{
			PreviousValue = oldValue;
			NewValue = newValue;
		}
	}
}
