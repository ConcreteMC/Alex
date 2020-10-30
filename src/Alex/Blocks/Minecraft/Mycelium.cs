namespace Alex.Blocks.Minecraft
{
	public class Mycelium : Block
	{
		public Mycelium() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 0.6f;

			BlockMaterial = Material.Grass;
		}
	}
}
