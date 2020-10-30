namespace Alex.Blocks.Minecraft
{
	public class Vine : Block
	{
		public Vine() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Vine;
			Hardness = 0.2f;
		}
	}
}
