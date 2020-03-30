namespace Alex.Blocks.Minecraft
{
	public class Dandelion : Block
	{
		public Dandelion() : base(1021)
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
