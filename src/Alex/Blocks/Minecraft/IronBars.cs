namespace Alex.Blocks.Minecraft
{
	public class IronBars : Block
	{
		public IronBars() : base(4120)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 5;
			BlockMaterial = Material.Iron;
		}
	}
}
