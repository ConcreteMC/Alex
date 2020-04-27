namespace Alex.Blocks.Minecraft
{
	public class Dispenser : Block
	{
		public Dispenser() : base(144)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 3.5f;
			BlockMaterial = Material.Circuits;
		}
	}
}
