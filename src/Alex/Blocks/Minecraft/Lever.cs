namespace Alex.Blocks.Minecraft
{
	public class Lever : Block
	{
		public Lever() : base()
		{
			Solid = true;
			Transparent = true;
			CanInteract = true;
			
			BlockMaterial = Material.Circuits;
		}
	}
}
