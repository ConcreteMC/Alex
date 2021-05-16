namespace Alex.Blocks.Minecraft
{
	public class HeavyWeightedPressurePlate : PressurePlate
	{
		public HeavyWeightedPressurePlate() : base()
		{
			Solid = true;
			Transparent = true;

			//Hardness = 0.5f;
			
			BlockMaterial  = Material.Iron.Clone().SetHardness(0.5f);
		}
	}
}
