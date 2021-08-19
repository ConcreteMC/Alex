using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Utils
{
	public interface IMapElement
	{
		Vector3 Position { get; }

		Texture2D GetTexture(GraphicsDevice device);
	}
}