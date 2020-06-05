using System;
using Alex.API.Input;
using Alex.Items;
using Alex.Utils.Inventories;

namespace Alex.Utils
{
    public class Inventory : InventoryBase
    {
	    protected byte _selectedSlot = 0;

	    public virtual int SelectedSlot
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
		       // MainHand = Slots[36 + value];
		       InvokeSelectedHotbarSlotChange(new SelectedSlotChangedEventArgs(oldValue, (byte)value));
			}
	    }

	    protected void InvokeSelectedHotbarSlotChange(SelectedSlotChangedEventArgs eventArgs)
	    {
		    SelectedHotbarSlotChanged?.Invoke(this, eventArgs);
	    }

	    public int InventoryOffset { get; set; } = 0;
	    public int HotbarOffset { get; set; } = 36;
	    public virtual Item MainHand
        {
	        get
	        {
		        return Slots[36 + _selectedSlot];
	        }
	        set
	        {
		        //Slots[36 + _selectedSlot] = value;
		        SetSlot(36 + _selectedSlot, value, true);
	        }
        }

        public virtual Item OffHand { get; set; }

        public int HelmetSlot { get; protected set; } = 5;
        public virtual Item Helmet
        {
	        get
	        {
		        return Slots[HelmetSlot];
	        }
	        set
	        {
		        SetSlot(HelmetSlot, value, true);
		      //  Slots[HelmetSlot] = value;
	        }
        }

        public int ChestSlot { get; protected set; } = 6;
        public virtual Item Chestplate
        {
	        get
	        {
		        return Slots[ChestSlot];
	        }
	        set
	        {
		        SetSlot(ChestSlot, value, true);
	        }
        }

        public int LeggingsSlot { get; protected set; } = 7;
        public virtual Item Leggings
        {
	        get
	        {
		        return Slots[LeggingsSlot];
	        }
	        set
	        {
		        SetSlot(LeggingsSlot, value, true);
	        }
        }
        
        public int BootsSlot { get; protected set; } = 8;
	    public virtual Item Boots
	    {
		    get
		    {
			    return Slots[BootsSlot];
		    }
		    set
		    {
			    SetSlot(BootsSlot, value, true);
		    }
	    }
	    
	    public event EventHandler<SelectedSlotChangedEventArgs> SelectedHotbarSlotChanged = null;

	    public Inventory(int slots) : base(slots)
	    {
		    
	    }

	    public virtual Item[] GetHotbar()
	    {
		    Item[] items = new Item[9];

		    for (int i = 0; i < 9; i++)
		    {
			    items[i] = this[36 + i];
		    }

		    return items;
	    }

	    internal void TriggerClosedEvent()
	    {
		    Closed?.Invoke(this, EventArgs.Empty);
	    }

	    public EventHandler Closed;
    }

	public class SlotChangedEventArgs : EventArgs
	{
		public int InventoryId;
		public int Index;
		public Item Value;

		public Item OldItem;
		
		public bool IsServerTransaction { get; set; }
		
		public SlotChangedEventArgs(int inventoryId, int index, Item value, Item oldItem, bool isServerTransaction)
		{
			InventoryId = inventoryId;
			Index = index;
			Value = value;
			OldItem = oldItem;
			IsServerTransaction = isServerTransaction;
		}
	}

	public class CursorChangedEventArgs : SlotChangedEventArgs
	{
		public MouseButton Button { get; }
		public CursorChangedEventArgs(int inventoryId, int index, Item value, Item oldItem, bool isServerTransaction, MouseButton button) : base(inventoryId, index, value, oldItem, isServerTransaction)
		{
			Button = button;
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
