namespace Alex.Blocks.Minecraft
{
	public class StoneSlab : Slab
	{
		public StoneSlab() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;
		}
	}
}
