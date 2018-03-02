using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui
{
	public class GuiManager
	{
		private Game Game { get; }

		public GuiRenderer Renderer { get; set; }

		public UiSkin Skin { get; set; }

		public UiContainer Root { get; private set; }

		public GuiManager(Game game)
		{
			Game = game;
			Renderer = new GuiRenderer(game.GraphicsDevice, game.GraphicsDevice.PresentationParameters.BackBufferWidth, game.GraphicsDevice.PresentationParameters.BackBufferHeight);
			Initialise();
		}

		private void Initialise()
		{
			Root = new UiContainer(Renderer.ScreenWidth, Renderer.ScreenHeight);
		}


		public void Update(GameTime gameTime)
		{
			UpdateInput(gameTime);
			UpdateLayout(gameTime);

			Root.Update(gameTime);
		}

		public void Draw(GameTime gameTime)
		{
			Renderer.Begin();
			Root.Draw(gameTime, Renderer);
			Renderer.End();
		}


		private void UpdateInput(GameTime gameTime)
		{

		}

		private void UpdateLayout(GameTime gameTime)
		{
			Root.UpdateLayout();
		}
	}
}
