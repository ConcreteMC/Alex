namespace Alex.Blocks.Minecraft
{
	public class BrewingStand : Block
	{
		public BrewingStand() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			LightValue = 1;

			Hardness = 0.5f;
			
			CanInteract = true;
		}
	}
}
