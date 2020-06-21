using Alex.Items;

namespace Alex.Utils.Inventories
{
	public class BedrockInventory : Inventory
	{
		public BedrockInventory(int slots) : base(slots)
		{
			HelmetSlot = 39;
			ChestSlot = 38;
			LeggingsSlot = 37;
			BootsSlot = 36;

			HotbarOffset = 0;
			InventoryOffset = 9;
		}

		protected byte _selectedSlot = 0;

		public override int SelectedSlot
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

		public override Item MainHand
		{
			get { return Slots[_selectedSlot]; }
			set
			{
				SetSlot(_selectedSlot, value, true);
			}
		}

		/// <inheritdoc />
		public override Item[] GetHotbar()
		{
			Item[] hotbar = new Item[9];

			for (int i = 0; i < hotbar.Length; i++)
			{
				hotbar[i] = Slots[i];
			}

			return hotbar;
		}
	}
}