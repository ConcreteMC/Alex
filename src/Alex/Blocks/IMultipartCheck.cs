using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks
{
	public interface IMultipartCheck
	{
		bool Passes(IBlockAccess world, Vector3 position, string rule, string value);

		bool Passes(string rule, string value);
	}
}