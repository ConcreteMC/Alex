namespace Alex.Blocks.Minecraft
{
	public class TallGrass : Block
	{
		public TallGrass() : base(6761)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = true;

			BlockMaterial = Material.Grass;
			Hardness = 0.6f;
		}
	}
}
