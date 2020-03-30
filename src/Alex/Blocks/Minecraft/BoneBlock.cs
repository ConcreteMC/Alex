namespace Alex.Blocks.Minecraft
{
	public class BoneBlock : Block
	{
		public BoneBlock() : base(8106)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
		}
	}
}
