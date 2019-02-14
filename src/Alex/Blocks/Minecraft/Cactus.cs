namespace Alex.Blocks.Minecraft
{
	public class Cactus : Block
	{
		public Cactus() : base(3335)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Cactus;
		}
	}
}
