namespace Alex.Blocks.Minecraft
{
	public class Dandelion : FlowerBase
	{
		public Dandelion() : base()
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
