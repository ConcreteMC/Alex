namespace Alex.Blocks.Minecraft
{
	public class RedstoneBlock : Block
	{
		public RedstoneBlock() : base(5593)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 5;;
		}
	}
}
