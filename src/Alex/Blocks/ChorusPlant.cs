using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class ChorusPlant : Block
	{
		public ChorusPlant() : base(7976)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
