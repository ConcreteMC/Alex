using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Input;
using Alex.Items;
using log4net;
using RocketUI.Input;

namespace Alex.Utils.Inventories
{
	public class CursorInventory
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(CursorInventory));

		public List<Item> Slots { get; } = Enumerable.Repeat((Item) new ItemAir(), 51).ToList();

		public Item Cursor
		{
			get => Slots[0];
			set => Slots[0] = value;
		}

		public CursorInventory()
		{
		}

		public void Clear()
		{
			for (int i = 0; i < Slots.Count; i++)
			{
				if (Slots[i] == null || Slots[i].Id != 0) Slots[i] = new ItemAir();
			}
		}
	}
	
    public class Inventory : InventoryBase
    {
	    private   InventoryBase   CraftingInventory { get; }      = new InventoryBase(5);
	    public    CursorInventory UiInventory       { get; set; } = new CursorInventory();
	    
	    protected byte            _selectedSlot = 0;

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
		        return Get(HotbarOffset + _selectedSlot);
	        }
	        set
	        {
		        //Slots[36 + _selectedSlot] = value;
		        SetSlot(HotbarOffset + _selectedSlot, value, true);
	        }
        }

	    public virtual Item OffHand     { get; set; }
	    public         int  OffHandSlot { get; set; }

        public int HelmetSlot { get; protected set; } = 5;
        public virtual Item Helmet
        {
	        get
	        {
		        return Get(HelmetSlot);
		        //   return Slots[HelmetSlot];
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
		        return Get(ChestSlot);
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
		        return Get(LeggingsSlot);
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
			    return Get(BootsSlot);
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
			    items[i] = Get(HotbarOffset + i);
		    }

		    return items;
	    }

	    public void SetCraftingSlot(int slot, Item item, bool serverTransaction)
	    {
		    CraftingInventory.SetSlot(slot, item, serverTransaction);
	    }
	    
	    public Item GetCraftingSlot(int slot)
	    {
		    return CraftingInventory[slot];
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
