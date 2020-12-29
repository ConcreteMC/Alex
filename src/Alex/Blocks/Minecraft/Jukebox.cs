namespace Alex.Blocks.Minecraft
{
	public class Jukebox : Block
	{
		public Jukebox() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			CanInteract = true;
			
			Hardness = 2f;
		}
	}
}
