namespace Alex.Blocks.Minecraft
{
	public class CobblestoneWall : Block
	{
		public CobblestoneWall() : base(5110)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Stone;
		}
	}
}
