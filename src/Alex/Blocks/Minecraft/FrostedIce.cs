namespace Alex.Blocks.Minecraft
{
	public class FrostedIce : Block
	{
		public FrostedIce() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			Hardness = 0.5f;
			BlockMaterial = Material.Ice;
		}
	}
}
