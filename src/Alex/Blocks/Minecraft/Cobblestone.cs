namespace Alex.Blocks.Minecraft
{
	public class Cobblestone : Block
	{
		public Cobblestone() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Stone;
			Hardness = 2;
		}
	}
}
