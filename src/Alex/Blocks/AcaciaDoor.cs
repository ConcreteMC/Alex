using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class AcaciaDoor : Block
	{
		public AcaciaDoor() : base(7790)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
