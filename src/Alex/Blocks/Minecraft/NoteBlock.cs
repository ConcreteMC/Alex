namespace Alex.Blocks.Minecraft
{
	public class NoteBlock : Block
	{
		public NoteBlock() : base(159)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			
			CanInteract = true;

			Hardness = 0.8f;
		}
	}
}
