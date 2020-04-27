namespace Alex.Blocks.Minecraft
{
	public class IronDoor : Door
	{
		public IronDoor() : base(3224)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			CanOpen = false;
			CanInteract = false;

			BlockMaterial = Material.Iron;
			Hardness = 5;
		}
	}
}
