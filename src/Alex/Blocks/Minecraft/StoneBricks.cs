namespace Alex.Blocks.Minecraft
{
	public class StoneBricks : Block
	{
		public StoneBricks() : base()
		{
			Solid = true;
			Transparent = false;
			IsFullCube = true;
			
			BlockMaterial = Material.Stone.Clone().SetHardness(1.5f);
		}
	}
}
