namespace Alex.Blocks.Minecraft
{
	public class CoalBlock : Block
	{
		public CoalBlock() : base(6750)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			Hardness = 5;
		}
	}
}
