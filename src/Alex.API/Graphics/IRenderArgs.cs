using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics
{

	public interface IRenderArgs
	{
		GameTime GameTime { get; }
		GraphicsDevice GraphicsDevice { get; }
		SpriteBatch SpriteBatch { get; }
	}
}
