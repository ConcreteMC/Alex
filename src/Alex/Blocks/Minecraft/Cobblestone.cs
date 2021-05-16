namespace Alex.Blocks.Minecraft
{
	public class Cobblestone : Block
	{
		public Cobblestone() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone.Clone().SetHardness(2);
		}
	}
}
