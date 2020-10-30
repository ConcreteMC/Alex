namespace Alex.Blocks.Minecraft
{
	public class EmeraldOre : Block
	{
		public EmeraldOre() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Ore;
		}
	}
}
