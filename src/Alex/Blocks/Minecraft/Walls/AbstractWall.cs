namespace Alex.Blocks.Minecraft
{
	public abstract class AbstractWall : Block
	{
		protected AbstractWall()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Stone;
		}
	}
}