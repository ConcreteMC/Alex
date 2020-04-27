namespace Alex.Blocks.Minecraft
{
	public class HeavyWeightedPressurePlate : Block
	{
		public HeavyWeightedPressurePlate() : base(5529)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			
			Hardness = 0.5f;
		}
	}
}
