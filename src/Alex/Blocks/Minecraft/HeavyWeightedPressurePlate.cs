namespace Alex.Blocks.Minecraft
{
	public class HeavyWeightedPressurePlate : PressurePlate
	{
		public HeavyWeightedPressurePlate() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			
			Hardness = 0.5f;
			
			BlockMaterial  = Material.Iron;
		}
	}
}
