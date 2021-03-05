namespace Alex.Blocks.Minecraft
{
	public class CobblestoneWall : AbstractWall
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
