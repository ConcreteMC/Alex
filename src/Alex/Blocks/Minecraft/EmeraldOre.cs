namespace Alex.Blocks.Minecraft
{
	public class EmeraldOre : Block
	{
		public EmeraldOre() : base(4640)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Ore;
		}
	}
}
