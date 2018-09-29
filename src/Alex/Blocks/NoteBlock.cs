using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
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
