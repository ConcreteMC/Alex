namespace Alex.Blocks.Minecraft
{
	public class Dropper : Block
	{
		public Dropper() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			Hardness = 3.5f;
			BlockMaterial = Material.Circuits;
			
			CanInteract = true;
		}
	}
}
