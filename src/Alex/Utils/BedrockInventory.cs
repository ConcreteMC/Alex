using Alex.Items;

namespace Alex.Utils
{
	public class BedrockInventory : Inventory
	{
		public BedrockInventory(int slots) : base(slots)
		{
			IsPeInventory = true;
		}

		public override Item MainHand
		{
			get { return Slots[_selectedSlot]; }
			set { Slots[_selectedSlot] = value; }
		}

		public override Item Helmet
		{
			get { return Slots[39]; }
			set { Slots[39] = value; }
		}

		public override Item Chestplate
		{
			get { return Slots[38]; }
			set { Slots[38] = value; }
		}

		public override Item Leggings
		{
			get { return Slots[37]; }
			set { Slots[37] = value; }
		}

		public override Item Boots
		{
			get { return Slots[36]; }
			set { Slots[36] = value; }
		}
	}
}