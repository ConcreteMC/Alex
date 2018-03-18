using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class MelonStem : Block
	{
		public MelonStem() : base(4170)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
