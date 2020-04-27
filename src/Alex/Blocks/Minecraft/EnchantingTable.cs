namespace Alex.Blocks.Minecraft
{
	public class EnchantingTable : Block
	{
		public EnchantingTable() : base(4522)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 5;
		}
	}
}
