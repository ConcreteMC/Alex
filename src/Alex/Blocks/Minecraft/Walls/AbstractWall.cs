namespace Alex.Blocks.Minecraft
{
	public abstract class AbstractWall : Block
	{
		protected AbstractWall()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Stone;
		}
	}
}