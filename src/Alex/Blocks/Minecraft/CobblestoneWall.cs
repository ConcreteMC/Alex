namespace Alex.Blocks.Minecraft
{
	public class CobblestoneWall : Block
	{
		public CobblestoneWall() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Stone;
		}
	}
}
