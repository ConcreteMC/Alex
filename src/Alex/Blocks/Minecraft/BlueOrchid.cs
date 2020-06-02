namespace Alex.Blocks.Minecraft
{
	public class BlueOrchid : Block
	{
		public BlueOrchid()
		{
			Solid = false;
			Transparent = true;

			IsFullBlock = false;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}