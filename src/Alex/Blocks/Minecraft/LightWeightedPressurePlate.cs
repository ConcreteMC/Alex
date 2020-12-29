namespace Alex.Blocks.Minecraft
{
	public class LightWeightedPressurePlate : Block
	{
		public LightWeightedPressurePlate() : base()
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
