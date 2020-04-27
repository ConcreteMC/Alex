namespace Alex.Blocks.Minecraft
{
	public class StoneSlab : Slab
	{
		public StoneSlab() : base(7206)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Stone;
			Hardness = 2f;
		}
	}
}
