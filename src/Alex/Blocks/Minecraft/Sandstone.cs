namespace Alex.Blocks.Minecraft
{
	public class Sandstone : Block
	{
		public Sandstone() : base(155)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			BlockMaterial = Material.Stone;
			Hardness = 0.8f;
		}
	}
}
