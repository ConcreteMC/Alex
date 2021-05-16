namespace Alex.Blocks.Minecraft
{
	public class NoteBlock : Block
	{
		public NoteBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsFullCube = true;
			
			CanInteract = true;

			BlockMaterial = Material.Stone.Clone().SetHardness(0.8f);
		}
	}
}
