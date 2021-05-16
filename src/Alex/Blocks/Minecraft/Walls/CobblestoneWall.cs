namespace Alex.Blocks.Minecraft
{
	public class CobblestoneWall : AbstractWall
	{
		public CobblestoneWall() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;
		}
	}
}
