namespace Alex.Blocks.Minecraft
{
	public class GoldBlock : Block
	{
		public GoldBlock() : base(1033)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 3;
		}
	}
}
