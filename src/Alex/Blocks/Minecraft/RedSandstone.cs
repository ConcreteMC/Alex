namespace Alex.Blocks.Minecraft
{
	public class RedSandstone : Block
	{
		public RedSandstone() : base(7084)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			BlockMaterial = Material.Stone;
			Hardness = 0.8f;
		}
	}
}
