namespace Alex.Blocks.Minecraft
{
	public class Trapdoor : OpenableBlockBase
	{
		public Trapdoor() : base()
		{
			Solid = true;
			IsFullCube = false;
			Transparent = true;
			CanInteract = true;
		}
	}
}