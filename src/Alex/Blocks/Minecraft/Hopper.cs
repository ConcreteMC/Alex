namespace Alex.Blocks.Minecraft
{
	public class Hopper : Block
	{
		public Hopper() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Iron;
		}
	}
}
