using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PuppeteerSharp.Input;
using RocketUI;
using Color = Microsoft.Xna.Framework.Color;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;
using Size = System.Drawing.Size;

namespace Alex.Gui.Elements.Web
{
	public class WebElement : RocketControl
	{
		readonly OffscreenBrowserRenderer _browserRenderer;

		public string Homepage { get; set; } = "https://google.com/";
		public Point TargetResolution { get; set; } = new Point(1920, 1080);
		public float Transparency { get; set; } = 0f;
		public WebElement()
		{
			_browserRenderer = new OffscreenBrowserRenderer();
		//	_browserRenderer.DataChanged += BrowserRenderer_DataChanged;

			CanFocus = true;
			CanHighlight = true;
		}

		private bool _didInit = false;
		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			if (_didInit)
				return;
			
			base.OnInit(renderer);
			
			var size = RenderBounds;

			AsyncHelpers.RunSync(
				() => _browserRenderer.MainAsync(
					GuiManager.GraphicsDevice, Alex.Instance.Window.Handle, Homepage, null,
					new System.Drawing.Size(TargetResolution.X, TargetResolution.Y)));

			Focus();

			_didInit = true;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_browserRenderer.Dispose();
			}
			
			base.Dispose(disposing);
		}

		private Point _previousSize = Point.Zero;
		/// <inheritdoc />
		protected override void OnUpdateLayout()
		{
			base.OnUpdateLayout();
		}

		/// <inheritdoc />
		protected override bool OnKeyInput(char character, Keys key)
		{
			if (Focused)
			{
				_browserRenderer.OnKeyInput(character, key);
				return true;
			}
			
			return base.OnKeyInput(character, key);
		}

		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			//base.OnDraw(graphics, gameTime);
			using (var branch = graphics.BranchContext(BlendState.NonPremultiplied))
			{
				graphics.Begin();
				
				var frame = _browserRenderer.CurrentFrame;

				if (frame != null)
				{
					graphics.SpriteBatch.Draw(frame, RenderBounds, frame.Bounds, Color.White * (1f - Transparency));
				}
				
				graphics.End();
			}
			//graphics.FillRectangle(RenderBounds, _webViewTexture, TextureRepeatMode.Stretch);
		}


		private MouseState _lastMouseState;
		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			var size = RenderBounds.Size;

			if (_previousSize != size)
			{
				size = GuiRenderer.Project(new Vector2(size.X, size.Y)).ToPoint();
				_browserRenderer.Resize(new Size(size.X, size.Y));
				_previousSize = size;
			}

			var mouseState = Mouse.GetState();

			if (mouseState != _lastMouseState)
			{
				var mousePosition = new Vector2(mouseState.X, mouseState.Y);
				mousePosition -= GuiRenderer.Project(RenderPosition);
				//mousePosition = GuiRenderer.Unproject(mousePosition);
				_browserRenderer.HandleMouseMove(mousePosition.X, mousePosition.Y);
				
				
				if (mouseState.LeftButton != _lastMouseState.LeftButton)
				{
					if (mouseState.LeftButton == ButtonState.Pressed)
						_browserRenderer.HandleMouseDown(mousePosition.X, mousePosition.Y, MouseButton.Left);
					else
						_browserRenderer.HandleMouseUp(mousePosition.X, mousePosition.Y, MouseButton.Left);
				}

				if (mouseState.RightButton != _lastMouseState.RightButton)
				{
					if (mouseState.RightButton == ButtonState.Pressed)
						_browserRenderer.HandleMouseDown(mousePosition.X, mousePosition.Y, MouseButton.Right);
					else
						_browserRenderer.HandleMouseUp(mousePosition.X, mousePosition.Y, MouseButton.Right);
				}

				if (mouseState.MiddleButton != _lastMouseState.MiddleButton)
				{
					if (mouseState.MiddleButton == ButtonState.Pressed)
						_browserRenderer.HandleMouseDown(mousePosition.X, mousePosition.Y, MouseButton.Middle);
					else
						_browserRenderer.HandleMouseUp(mousePosition.X, mousePosition.Y, MouseButton.Middle);
				}

				if (mouseState.ScrollWheelValue != _lastMouseState.ScrollWheelValue
				    || mouseState.HorizontalScrollWheelValue != _lastMouseState.HorizontalScrollWheelValue)
				{
					var deltaX = _lastMouseState.HorizontalScrollWheelValue - mouseState.HorizontalScrollWheelValue;
					var deltaY = _lastMouseState.ScrollWheelValue - mouseState.ScrollWheelValue;

					_browserRenderer.HandleMouseScroll(deltaX, deltaY);
				}

				_lastMouseState = mouseState;
			}

			_browserRenderer.Update();

			//	BrowserRenderer.PullLatestDataIfChanged();
		}

		public Uri Source
		{
			get
			{
				return new Uri(this._browserRenderer.Browser.Target.Url);
			}
			set
			{
				var str = value.ToString();
				_browserRenderer.LoadPageAsync(str).Wait();
			}
		}

		public string Title { get; set; } = "N/A";
	}
}