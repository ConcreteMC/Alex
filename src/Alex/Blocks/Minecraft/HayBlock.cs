namespace Alex.Blocks.Minecraft
{
	public class HayBlock : Block
	{
		public HayBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 0.5f;
			
			BlockMaterial = Material.Grass;
			
		}
	}
}
