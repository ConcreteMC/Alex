using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Input;
using Alex.Graphics.UI.Rendering;
using Alex.Graphics.UI.Themes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.UI
{
	public class UiManager
	{
		private Game Game { get; }

		public UiTheme Theme { get; set; } = new UiTheme();

		public UiScaledResolution ScaledResolution { get; private set; }

		public UiRenderer Renderer { get; private set; }

		public UiRoot Root { get; private set; }

		private bool _doResize = false;

		private IInputManager Input { get; }

		public UiManager(Game game)
		{
			Game = game;

			Input = new UiInputManager(this);

		}

		private void OnScaleChanged(object sender, UiScaleEventArgs e)
		{
			Renderer.SetVirtualSize(e.ScaledWidth, e.ScaledHeight, e.ScaleFactor);
			
			_doResize = true;
		}

		public Point PointToScreen(Point point)
		{
			return Renderer?.PointToScreen(point) ?? point;
		}

		public void Init(GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			Renderer = new UiRenderer(this, graphics, spriteBatch);

			ScaledResolution              =  new UiScaledResolution(Game);
			ScaledResolution.ScaleChanged += OnScaleChanged;

			Root = new UiRoot(Renderer);
			
			_doResize = true;

			Root.Activate(Input);
		}
		
		public void Update(GameTime gameTime)
		{
			if (_doResize)
			{
				ScaledResolution.Update();
				Root.UpdateLayoutInternal();

				_doResize = false;
			}

			Input.Update(gameTime);
			Root.Update(gameTime);
		}

		public void Draw(GameTime gameTime)
		{
			Renderer.BeginDraw();
			Root.Draw(gameTime, Renderer);
			Renderer.EndDraw();
		}
	}
}
