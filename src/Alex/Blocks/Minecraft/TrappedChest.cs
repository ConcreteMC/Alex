namespace Alex.Blocks.Minecraft
{
	public class TrappedChest : Chest
	{
		public TrappedChest() : base()
		{
			Solid = true;
			Transparent = true;

			CanInteract = true;
		}
	}
}
