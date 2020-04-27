namespace Alex.Blocks.Minecraft
{
	public class PurpurBlock : Block
	{
		public PurpurBlock() : base(7983)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 1.5f;
		}
	}
}
