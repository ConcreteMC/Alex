namespace Alex.Blocks.Minecraft
{
	public class CommandBlock : Block
	{
		public CommandBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
			BlockMaterial  = Material.Iron;
		}
	}
}
