namespace Alex.Blocks.Minecraft
{
	public class SweetBerryBush : Block
	{
		public SweetBerryBush()
		{
			Solid = false;
			Transparent = true;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}