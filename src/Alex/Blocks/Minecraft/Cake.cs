namespace Alex.Blocks.Minecraft
{
	public class Cake : Block
	{
		public Cake() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Cake;
		}
	}
}
