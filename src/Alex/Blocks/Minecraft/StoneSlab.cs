namespace Alex.Blocks.Minecraft
{
	public class StoneSlab : Slab
	{
		public StoneSlab() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Stone;
			Hardness = 2f;
		}
	}
}
