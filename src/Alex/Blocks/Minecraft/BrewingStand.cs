namespace Alex.Blocks.Minecraft
{
	public class BrewingStand : Block
	{
		public BrewingStand() : base()
		{
			Solid = true;
			Transparent = true;
			Luminance = 1;
			IsFullCube = false;

			CanInteract = true;
		}
	}
}
