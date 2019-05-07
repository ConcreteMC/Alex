namespace Alex.Blocks.Minecraft
{
	public class NoteBlock : Block
	{
		public NoteBlock() : base(159)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
