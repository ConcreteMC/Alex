namespace Alex.Blocks.Minecraft
{
	public class Poppy : FlowerBase
	{
		public Poppy() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}
