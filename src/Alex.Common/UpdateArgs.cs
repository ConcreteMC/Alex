using Alex.Common.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Common
{
	public class UpdateArgs : IUpdateArgs
	{
		public GameTime GameTime { get; set; }
		public GraphicsDevice GraphicsDevice { get; set; }
		public ICamera Camera { get; set; }
	}
}
