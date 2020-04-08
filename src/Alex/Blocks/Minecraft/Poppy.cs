namespace Alex.Blocks.Minecraft
{
	public class Poppy : Block
	{
		public Poppy() : base(1022)
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
