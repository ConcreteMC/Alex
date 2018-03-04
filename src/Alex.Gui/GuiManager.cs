using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Input;
using Alex.Gui.Rendering;
using Alex.Gui.Themes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui
{
	public class GuiManager
	{
		private Game Game { get; }

		public GuiRenderer Renderer { get; set; }

		public UiRoot Root { get; private set; }

		private bool _doResize = false;

		private IInputManager Input { get; }

		public GuiManager(Game game)
		{
			Game = game;
			Root = new UiRoot(game.Window.ClientBounds.Width, game.Window.ClientBounds.Height);

			game.Window.ClientSizeChanged += WindowOnClientSizeChanged;
			Input = new UiInputManager();
		}

		private void WindowOnClientSizeChanged(object sender, EventArgs eventArgs)
		{
			var width = Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
			var height = Game.GraphicsDevice.PresentationParameters.BackBufferHeight;

			Renderer.ResetScreenSize(width, height);
			Root.Width = width;
			Root.Height = height;

			_doResize = true;
		}

		public void Init(GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			Renderer = new GuiRenderer(graphics, spriteBatch, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight);
			Root = new UiRoot(Renderer.ScreenWidth, Renderer.ScreenHeight);
			_doResize = true;

			Root.Activate(Input);
		}
		
		public void Update(GameTime gameTime)
		{
			if (_doResize)
			{
				UpdateLayout(gameTime);
				_doResize = false;
			}

			Input.Update(gameTime);
			Root.Update(gameTime);
		}

		public void Draw(GameTime gameTime)
		{
			Renderer.Begin();
			Root.Draw(gameTime, Renderer);
			Renderer.End();
		}
		
		private void UpdateLayout(GameTime gameTime)
		{
			Root.UpdateSize();
			Root.UpdateLayout();
		}
	}
}
