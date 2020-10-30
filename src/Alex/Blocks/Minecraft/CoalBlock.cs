namespace Alex.Blocks.Minecraft
{
	public class CoalBlock : Block
	{
		public CoalBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 5;
		}
	}
}
