namespace Alex.Blocks.Minecraft
{
	public class StructureBlock : Block
	{
		public StructureBlock() : base(8378)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
		}
	}
}
