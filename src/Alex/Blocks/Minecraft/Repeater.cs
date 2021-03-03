namespace Alex.Blocks.Minecraft
{
	public class Repeater : RedstoneBase
	{
		public Repeater()
		{
			BlockMaterial = Material.Circuits;
			IsFullBlock = false;
			CanInteract = true;
		}
	}
}