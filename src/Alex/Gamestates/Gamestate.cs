using System.Collections.Generic;
using System.Linq;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class Gamestate
	{
		public Dictionary<string, UIComponent> Controls { get; set; }

		public Gamestate()
		{
			Controls = new Dictionary<string, UIComponent>();
		}

		public Vector2 CenterScreen
		{
			get
			{
				return new Vector2((Alex.Instance.GraphicsDevice.Viewport.Width/2f),
					(Alex.Instance.GraphicsDevice.Viewport.Height/2f));
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

	public class RenderArgs
	{
		public GameTime GameTime;
		public GraphicsDevice GraphicsDevice;
		public SpriteBatch SpriteBatch;
	}
}
