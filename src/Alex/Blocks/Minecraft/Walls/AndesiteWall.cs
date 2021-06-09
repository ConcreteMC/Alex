namespace Alex.Blocks.Minecraft
{
	public class AndesiteWall : AbstractWall
	{
		public AndesiteWall()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;			
		}
	}
}