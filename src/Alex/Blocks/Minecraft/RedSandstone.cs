namespace Alex.Blocks.Minecraft
{
	public class RedSandstone : Block
	{
		public RedSandstone() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			BlockMaterial = Material.Stone;
			Hardness = 0.8f;
		}
	}
}
