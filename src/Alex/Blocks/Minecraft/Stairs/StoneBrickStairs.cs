namespace Alex.Blocks.Minecraft
{
	public class StoneBrickStairs : Stairs
	{
		public StoneBrickStairs() : base(4333)
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;
		}
	}
}
