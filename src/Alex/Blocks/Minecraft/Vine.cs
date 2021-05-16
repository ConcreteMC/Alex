namespace Alex.Blocks.Minecraft
{
	public class Vine : Block
	{
		public Vine() : base()
		{
			Solid = false;
			Transparent = true;

			BlockMaterial = Material.Vine;
		}
	}
}
