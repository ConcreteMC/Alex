namespace Alex.Blocks.Minecraft
{
	public class BrewingStand : Block
	{
		public BrewingStand() : base()
		{
			Solid = true;
			Transparent = true;
			LightValue = 1;
			IsFullCube = false;

			CanInteract = true;
		}
	}
}
