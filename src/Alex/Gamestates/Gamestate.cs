using System.Collections.Generic;
using System.Linq;
using Alex.Gui;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class Gamestate
	{
		public Dictionary<string, UIComponent> Controls { get; set; }

		public GuiManager Gui { get; }

		protected GraphicsDevice Graphics { get; }

		protected Game Game { get; }

		public Gamestate(Game game)
		{
			Game = game;
			Graphics = game.GraphicsDevice;
			Controls = new Dictionary<string, UIComponent>();

			Gui = new GuiManager(game);
		}

		public Viewport Viewport => Graphics.Viewport;

		public Vector2 CenterScreen
		{
			get
			{
				return new Vector2((Graphics.Viewport.Width / 2f),
					(Graphics.Viewport.Height / 2f));
			}
		}

		public void Initialise(RenderArgs args)
		{
			Gui.Init(args.GraphicsDevice, args.SpriteBatch);
			Init(args);
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
			Gui.Draw(args.GameTime);
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
			Gui.Update(gametime);
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
