namespace Alex.Blocks.Minecraft
{
	public class SweetBerryBush : Block
	{
		public SweetBerryBush()
		{
			Solid = false;
			Transparent = true;
			IsFullBlock = false;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}