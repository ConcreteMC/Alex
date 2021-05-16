namespace Alex.Blocks.Minecraft
{
	public class SandstoneStairs : Stairs
	{
		public SandstoneStairs() : base(4571)
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;
		}
	}
}
