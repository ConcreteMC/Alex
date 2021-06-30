using System;
using System.Collections.Generic;
using Alex.Items;
using Alex.Networking.Java.Packets.Play;
using MiNET.Utils;
using NLog;
using RocketUI.Input;

namespace Alex.Utils.Inventories
{
	public class InventoryBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(InventoryBase));
		public int InventoryId { get; set; } = 0;
		
		protected virtual Item[] Slots     { get; }
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

			_cursor = new ItemAir()
			{
				Count = 0
			};
		}

		public bool ReportTransaction = false;
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

			Set(index, value, !isServerTransaction);
		}

		protected void InvokeSlotChanged(SlotChangedEventArgs eventArgs)
		{
			SlotChanged?.Invoke(this, eventArgs);
		}

		protected virtual Item Get(int index)
		{
			return Slots[index];
		}

		protected virtual void Set(int index, Item value, bool isClientTransaction)
		{
			var oldValue = Slots[index];
			Slots[index] = value;
			
			InvokeSlotChanged(new SlotChangedEventArgs(InventoryId, index, value, oldValue, !isClientTransaction));
		}
		
		public Item this[int index]
		{
			get
			{
				return Get(index);
			}
			set
			{
				Set(index, value, true);
			}
		}

		public Dictionary<short, (ClickWindowPacket packet, SlotChangedEventArgs slot, bool isCursorTransaction)>
			UnconfirmedWindowTransactions { get; } =
			new Dictionary<short, (ClickWindowPacket packet, SlotChangedEventArgs slot, bool isCursorTransaction)>();
		
		public int ActionNumber { get; set; } = 1;
		public event EventHandler<CursorChangedEventArgs> CursorChanged = null;
		public virtual void SetCursor(Item item, bool isServerTransaction, int index = -2, MouseButton button = MouseButton.Left)
		{
			var oldValue = _cursor;
			Cursor = item;
			
			InvokeCursorChanged(new CursorChangedEventArgs(InventoryId, index, item, oldValue, isServerTransaction, button));
		}

		protected void InvokeCursorChanged(CursorChangedEventArgs args)
		{
			CursorChanged?.Invoke(this, args);
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
				_cursor = value;
			}
		}
	}
}