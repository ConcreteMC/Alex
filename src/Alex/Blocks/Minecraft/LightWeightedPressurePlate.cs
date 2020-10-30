namespace Alex.Blocks.Minecraft
{
	public class LightWeightedPressurePlate : Block
	{
		public LightWeightedPressurePlate() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			Hardness = 0.5f;
			
			BlockMaterial  = Material.Iron;
		}
	}
}
