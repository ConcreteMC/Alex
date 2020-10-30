namespace Alex.Blocks.Minecraft
{
	public class HeavyWeightedPressurePlate : Block
	{
		public HeavyWeightedPressurePlate() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 0.5f;
			
			BlockMaterial  = Material.Iron;
		}
	}
}
