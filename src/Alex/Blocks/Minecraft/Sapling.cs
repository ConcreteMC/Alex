namespace Alex.Blocks.Minecraft
{
	public class Sapling : Block
	{
		public Sapling(WoodType woodType)
		{
			Transparent = true;
			Solid = false;
			
			BlockMaterial = Material.Plants;
		}
	}
}