namespace Alex.Blocks.Minecraft
{
	public class AndesiteWall : Block
	{
		public AndesiteWall()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Stone;			
		}
	}
}