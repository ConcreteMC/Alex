namespace Alex.Blocks.Minecraft
{
	public class Beacon : Block
	{
		public Beacon() : base()
		{
			Solid = true;
			Transparent = true;
			LightValue = 15;
			CanInteract = true;
			
			//Hardness = 3;

			BlockMaterial = Material.Glass.Clone().SetHardness(3f);
		}
	}
}
