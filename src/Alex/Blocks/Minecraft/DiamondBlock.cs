namespace Alex.Blocks.Minecraft
{
	public class DiamondBlock : Block
	{
		public DiamondBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 5;
		}
	}
}
