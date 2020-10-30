namespace Alex.Blocks.Minecraft
{
	public class StructureBlock : Block
	{
		public StructureBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
		}
	}
}
