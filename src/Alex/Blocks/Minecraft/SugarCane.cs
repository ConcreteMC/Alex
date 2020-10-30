namespace Alex.Blocks.Minecraft
{
	public class SugarCane : Block
	{
		public SugarCane() : base()
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
