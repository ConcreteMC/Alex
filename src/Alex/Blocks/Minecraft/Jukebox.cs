namespace Alex.Blocks.Minecraft
{
	public class Jukebox : Block
	{
		public Jukebox() : base()
		{
			Solid = true;
			Transparent = false;
			IsFullCube = true;
			CanInteract = true;
		}
	}
}