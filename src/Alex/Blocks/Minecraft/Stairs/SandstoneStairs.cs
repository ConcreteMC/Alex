namespace Alex.Blocks.Minecraft
{
	public class SandstoneStairs : Stairs
	{
		public SandstoneStairs() : base(4571)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Stone;
			Hardness = 0.8f;
		}
	}
}
