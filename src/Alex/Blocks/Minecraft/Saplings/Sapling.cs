namespace Alex.Blocks.Minecraft.Saplings
{
	public class Sapling : Block
	{
		public WoodType WoodType { get; set; }
		public Sapling(WoodType woodType)
		{
			WoodType = woodType;
			
			Transparent = true;
			Solid = false;
			IsFullCube = false;
			
			BlockMaterial = Material.Plants;
		}
	}
}