using System.Collections.Generic;
using System.Linq;
using Alex.Gui;
using Alex.Rendering.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class GameState
	{
		public Dictionary<string, UIComponent> Controls { get; set; }

		public UiRoot Gui { get; private set; }

		protected GraphicsDevice Graphics { get; }

		protected Alex Alex { get; }

		public GameState(Alex alex)
		{
			Alex = alex;
			Graphics = alex.GraphicsDevice;
			Controls = new Dictionary<string, UIComponent>();

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

		public void Load(RenderArgs args)
		{
			Gui = new UiRoot();
			Alex.GuiManager.Root.Controls.Add(Gui);
			OnLoad(args);
		}

		public void Unload()
		{
			Gui.Container.Controls.Remove(Gui);
			OnUnload();
		}

		public void Draw2D(RenderArgs args)
		{
			OnDraw2D(args);

			foreach (var control in Controls.Values.ToArray())
			{
				control.Render(args);
			}
		}

		public void Draw3D(RenderArgs args)
		{
			OnDraw3D(args);
		}

		public void Update(GameTime gameTime)
		{
			OnUpdate(gameTime);

			foreach (var control in Controls.Values.ToArray())
			{
				control.Update(gameTime);
			}
		}


		public void Show()
		{

			OnShow();
		}


		public void Hide()
		{
			OnHide();
		}
		
		protected virtual void OnShow() { }
		protected virtual void OnHide() { }

		protected virtual void OnLoad(RenderArgs args) { }
		protected virtual void OnUnload() { }

		protected virtual void OnUpdate(GameTime gameTime) { }

		protected virtual void OnDraw2D(RenderArgs args) { }
		protected virtual void OnDraw3D(RenderArgs args) { }
	}

	public class RenderArgs
	{
		public GameTime GameTime;
		public GraphicsDevice GraphicsDevice;
		public SpriteBatch SpriteBatch;
	}
}
