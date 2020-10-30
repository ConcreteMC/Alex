namespace Alex.Blocks.Minecraft
{
	public class Dirt : Block
	{
		public Dirt() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Ground;
			Hardness = 0.5f;
		}
	}
}
