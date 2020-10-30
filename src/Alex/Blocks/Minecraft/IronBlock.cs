namespace Alex.Blocks.Minecraft
{
	public class IronBlock : Block
	{
		public IronBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 5;
			BlockMaterial = Material.Iron;
		}
	}
}
