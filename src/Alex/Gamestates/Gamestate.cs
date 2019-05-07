using Alex.API.GameStates;
using Alex.API.Graphics;
using Alex.API.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.GameStates
{
	public class GameState : IGameState
	{
		public GuiScreen Gui { get; protected set; }

		protected GraphicsDevice Graphics { get; }

		protected Alex Alex { get; }

		public IGameState ParentState { get; set; } = null;
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

		public void Load(IRenderArgs args)
		{
			OnLoad(args);

			if (Gui != null)
			{
				Gui.InvalidateLayout(true);
			}
		}

		public void Unload()
		{
			OnUnload();
		}

		public void Draw(IRenderArgs args)
		{
			OnDraw(args);

			//if (Gui != null)
			//{
			//	Gui.Draw(Alex.GuiManager.GuiSpriteBatch, args.GameTime);
			//}
		}

		public void Update(GameTime gameTime)
		{
			OnUpdate(gameTime);

			if (Gui != null)
			{
				Gui.Update(gameTime);
			}
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

		protected virtual void OnLoad(IRenderArgs args) { }
		protected virtual void OnUnload() { }

		protected virtual void OnUpdate(GameTime gameTime) { }
		protected virtual void OnDraw(IRenderArgs args) { }
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
