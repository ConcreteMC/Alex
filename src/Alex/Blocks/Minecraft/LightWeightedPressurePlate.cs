namespace Alex.Blocks.Minecraft
{
	public class LightWeightedPressurePlate : Block
	{
		public LightWeightedPressurePlate() : base(5513)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			Hardness = 0.5f;
		}
	}
}
