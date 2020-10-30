namespace Alex.Blocks.Minecraft
{
	public class RedstoneBlock : Block
	{
		public RedstoneBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 5;;
		}
	}
}
