using Alex.Blocks.Materials;

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

			BlockMaterial = Material.Stone.Clone().WithHardness(0.8f);
		}
	}
}
