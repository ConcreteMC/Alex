namespace Alex.Blocks.Minecraft
{
	public class GoldBlock : Block
	{
		public GoldBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 3;
		}
	}
}
