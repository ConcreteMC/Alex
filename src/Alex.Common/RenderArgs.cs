using Alex.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Common
{
	public class RenderArgs : IRenderArgs
	{
		public GameTime GameTime { get; set; }
		public GraphicsDevice GraphicsDevice { get; set; }
		public SpriteBatch SpriteBatch { get; set; }
		public ICamera Camera { get; set; }
	}
}