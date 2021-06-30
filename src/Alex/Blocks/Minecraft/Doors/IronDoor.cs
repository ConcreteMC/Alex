namespace Alex.Blocks.Minecraft.Doors
{
	public class IronDoor : Door
	{
		public IronDoor() : base(3224)
		{
			Solid = true;
			Transparent = true;

			CanOpen = false;
			CanInteract = false;

			BlockMaterial = Material.Iron;
		}
	}
}
