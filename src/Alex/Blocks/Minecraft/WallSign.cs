namespace Alex.Blocks.Minecraft
{
	public class WallSign : Block
	{
		public WallSign() : base(3180)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 1;
			BlockMaterial = Material.Wood;
		}
	}
}
