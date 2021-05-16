namespace Alex.Blocks.Minecraft
{
	public class Netherrack : Block
	{
		public Netherrack() : base()
		{
			Solid = true;
			Transparent = false;
			BlockMaterial = Material.Stone.SetHardness(0.4f);
		}
	}
}
