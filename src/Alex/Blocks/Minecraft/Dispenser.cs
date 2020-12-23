namespace Alex.Blocks.Minecraft
{
	public class Dispenser : Block
	{
		public Dispenser() : base()
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
