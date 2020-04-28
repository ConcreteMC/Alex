using Alex.Items;

namespace Alex.Utils
{
	public class BedrockInventory : Inventory
	{
		public BedrockInventory(int slots) : base(slots)
		{
			IsPeInventory = true;
			HelmetSlot = 39;
			ChestSlot = 38;
			LeggingsSlot = 37;
			BootsSlot = 36;
		}

		public override Item MainHand
		{
			get { return Slots[_selectedSlot]; }
			set { Slots[_selectedSlot] = value; }
		}
	}
}