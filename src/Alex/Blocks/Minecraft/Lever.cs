namespace Alex.Blocks.Minecraft
{
	public class Lever : Block
	{
		public Lever() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			CanInteract = true;
			
			BlockMaterial = Material.Circuits;
			Hardness = 0.5f;
		}
	}
}
