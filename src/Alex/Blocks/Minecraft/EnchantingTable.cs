namespace Alex.Blocks.Minecraft
{
	public class EnchantingTable : Block
	{
		public EnchantingTable() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 5;

			CanInteract = true;
		}
	}
}
