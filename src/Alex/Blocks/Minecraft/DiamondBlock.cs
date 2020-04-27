namespace Alex.Blocks.Minecraft
{
	public class DiamondBlock : Block
	{
		public DiamondBlock() : base(2959)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 5;
		}
	}
}
