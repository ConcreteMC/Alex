using Alex.API.Data.Options;
using Alex.API.GameStates;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class GameState : IGameState
	{
		public GuiScreen Gui { get; protected set; }

		protected GraphicsDevice Graphics { get; }

		protected Alex Alex { get; }

		private bool IsLoaded { get; set; }
		private bool IsShown { get; set; }

		public IGameState ParentState { get; set; } = null;
		
		private IOptionsProvider OptionsProvider { get; }
		public AlexOptions Options => OptionsProvider.AlexOptions;
		public GameState(Alex alex)
		{
			Alex = alex;
			Graphics = alex.GraphicsDevice;
			OptionsProvider = GetService<IOptionsProvider>();
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
			if(IsLoaded) return;
			IsLoaded = true;

			OnLoad(args);

			if (Gui != null)
			{
				Gui.InvalidateLayout(true);
			}
		}

		public void Unload()
		{
			if(!IsLoaded) return;
			IsLoaded = false;

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
			if(!IsLoaded) return;

			OnUpdate(gameTime);

			if (Gui != null)
			{
				Gui.Update(gameTime);
			}
		}

		public void Show()
		{
			if(IsShown) return;

			if (Gui != null)
			{
				Alex.GuiManager.AddScreen(Gui);
			}
			OnShow();

			IsShown = true;
		}

		public void Hide()
		{
			if(!IsShown) return;
			IsShown = false;

			OnHide();
			
			if (Gui != null)
			{
				Alex.GuiManager.RemoveScreen(Gui);
			}
		}

		protected TService GetService<TService>() where TService : class
		{
			return Alex.Services.GetRequiredService<TService>();
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
