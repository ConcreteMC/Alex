namespace Alex.Blocks.Minecraft
{
	public class Dropper : Block
	{
		public Dropper() : base()
		{
			Solid = true;
			Transparent = false;
			
			BlockMaterial = Material.Circuits.Clone().SetHardness(3.5f);
			
			CanInteract = true;
		}
	}
}
