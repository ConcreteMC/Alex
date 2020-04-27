namespace Alex.Blocks.Minecraft
{
	public class Piston : Block
	{
		public Piston() : base(963)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Piston;
			Hardness = 0.5f;
		}
	}
}
