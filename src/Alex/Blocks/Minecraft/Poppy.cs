namespace Alex.Blocks.Minecraft
{
	public class Poppy : FlowerBase
	{
		public Poppy() : base()
		{
			Solid = false;
			Transparent = true;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}
