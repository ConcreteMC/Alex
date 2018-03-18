using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Bedrock : Block
	{
		public Bedrock() : base(33)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
