using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class ActivatorRail : Block
	{
		public ActivatorRail() : base(5696)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
