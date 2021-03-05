namespace Alex.Blocks.Minecraft
{
	public class RedSandstoneStairs : Stairs
	{
		public RedSandstoneStairs() : base(7098)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			BlockMaterial = Material.Stone;
		}
	}
}
