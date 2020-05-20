namespace Alex.Blocks.Minecraft
{
	public class Chest : Block
	{
		public Chest() : base(1639)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			CanInteract = true;
		}
	}
}
