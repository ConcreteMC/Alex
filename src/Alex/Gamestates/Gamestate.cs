using Alex.API.Graphics;
using Alex.API.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class GameState
	{
		public GuiScreen Gui { get; protected set; }

		protected GraphicsDevice Graphics { get; }

		protected Alex Alex { get; }

		public GameState ParentState { get; internal set; } = null;
		public GameState(Alex alex)
		{
			Alex = alex;
			Graphics = alex.GraphicsDevice;
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
			OnLoad(args);

			if (Gui != null)
			{
				Gui.UpdateLayout();
			}
		}

		public void Unload()
		{
			OnUnload();
		}

		public void Draw2D(RenderArgs args)
		{
			OnDraw2D(args);
		}

		public void Draw3D(RenderArgs args)
		{
			OnDraw3D(args);
		}

		public void Update(GameTime gameTime)
		{
			OnUpdate(gameTime);
		}

		public void Show()
		{
			if (Gui != null)
			{
				Alex.GuiManager.AddScreen(Gui);
			}
			OnShow();
		}

		public void Hide()
		{
			OnHide();
			
			if (Gui != null)
			{
				Alex.GuiManager.RemoveScreen(Gui);
			}
		}

		protected TService GetService<TService>() where TService : class
		{
			return Alex.Services.GetService<TService>();
		}
		
		protected virtual void OnShow() { }
		protected virtual void OnHide() { }

		protected virtual void OnLoad(RenderArgs args) { }
		protected virtual void OnUnload() { }

		protected virtual void OnUpdate(GameTime gameTime) { }

		protected virtual void OnDraw2D(RenderArgs args) { }
		protected virtual void OnDraw3D(RenderArgs args) { }
	}

	public class RenderArgs : IRenderArgs
	{
		public GameTime GameTime { get; set; }
		public GraphicsDevice GraphicsDevice { get; set; }
		public SpriteBatch SpriteBatch { get; set; }
		public ICamera Camera { get; set; }
	}

	public class UpdateArgs : IUpdateArgs
	{
		public GameTime GameTime { get; set; }
		public GraphicsDevice GraphicsDevice { get; set; }
		public ICamera Camera { get; set; }
	}
}
