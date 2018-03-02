using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class Gamestate
	{
		public Dictionary<string, UIComponent> Controls { get; set; }

        protected GraphicsDevice Graphics { get; }
		public Gamestate(GraphicsDevice graphics)
		{
		    Graphics = graphics;
			Controls = new Dictionary<string, UIComponent>();
		}

	    public Viewport Viewport => Graphics.Viewport;

		public Vector2 CenterScreen
		{
			get
			{
				return new Vector2((Graphics.Viewport.Width/2f),
					(Graphics.Viewport.Height/2f));
			}
		}

		public virtual void Init(RenderArgs args)
		{
		}

		public virtual void Stop()
		{
		}

		public void Rendering2D(RenderArgs args)
		{
			Render2D(args);
			foreach (var control in Controls.Values.ToArray())
			{
				control.Render(args);
			}
		}

		public virtual void Render2D(RenderArgs args)
		{
		}

		public void Rendering3D(RenderArgs args)
		{
			Render3D(args);
		}

		public virtual void Render3D(RenderArgs args)
		{
		}

		public void UpdateCall(GameTime gametime)
		{
			OnUpdate(gametime);
			foreach (var control in Controls.Values.ToArray())
			{
				control.Update(gametime);
			}
		}

		public virtual void OnUpdate(GameTime gameTime)
		{
		}
	}

	public class RenderArgs : IRenderArgs
	{
		public GameTime GameTime { get; set; }
		public GraphicsDevice GraphicsDevice { get; set; }
		public SpriteBatch SpriteBatch { get; set; }
	}
}
