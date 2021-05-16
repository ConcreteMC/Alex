namespace Alex.Blocks.Minecraft
{
	public class Piston : Block
	{
		public Piston() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Piston;
		}
	}
}
