namespace Alex.Blocks.Minecraft
{
	public class Anvil : Block
	{
		public Anvil() : base(5477)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			BlockMaterial = Material.Anvil;
			Hardness = 5;
			
			CanInteract = true;
		}
	}
}
