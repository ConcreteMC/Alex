namespace Alex.Blocks.Minecraft
{
	public class LightWeightedPressurePlate : Block
	{
		public LightWeightedPressurePlate() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial  = Material.Iron.Clone().SetHardness(0.5f);
		}
	}
}
