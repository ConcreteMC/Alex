namespace Alex.Blocks.Minecraft
{
	public class TrappedChest : Chest
	{
		public TrappedChest() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			
			CanInteract = true;
		}
	}
}
