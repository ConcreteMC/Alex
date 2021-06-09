using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog.Common;
using NLog.Fluent;
using PuppeteerSharp.Input;
using RocketUI;
using Color = Microsoft.Xna.Framework.Color;
using GpuResourceManager = Alex.Common.Graphics.GpuResources.GpuResourceManager;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Size = System.Drawing.Size;

namespace Alex.Gui.Elements.Web
{
	public class WebElement : RocketControl
	{
		readonly OffscreenBrowserRenderer _browserRenderer;

		public string Homepage { get; set; } = "https://google.com/";
		public WebElement()
		{
			_browserRenderer = new OffscreenBrowserRenderer();
		//	_browserRenderer.DataChanged += BrowserRenderer_DataChanged;

			CanFocus = true;
			CanHighlight = true;
		}


		private void BrowserRenderer_DataChanged(object obj)
		{
		//	Settings = (ShapeSettings)obj;
			//AddOrRemoveShapes();
		}

		private bool _didInit = false;
		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			if (_didInit)
				return;
			
			base.OnInit(renderer);
			
			var size = RenderBounds;
			AsyncHelpers.RunSync(() => _browserRenderer.MainAsync(GuiManager.GraphicsDevice, Alex.Instance.Window.Handle, 
				Homepage,
				null,
				new System.Drawing.Size(size.Width, size.Height)));

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

		private RocketUI.Size _previousSize = RocketUI.Size.Zero;
		/// <inheritdoc />
		protected override void OnUpdateLayout()
		{
			base.OnUpdateLayout();
		}

		/// <inheritdoc />
		protected override void OnCursorDown(Point cursorPosition)
		{
			base.OnCursorDown(cursorPosition);
			Log.Info($"Cursor down, {cursorPosition}");
			
			//_browserRenderer.HandleMouseDown(cursorPosition.X, cursorPosition.Y, MouseButton.Left);
		}

		/// <inheritdoc />
		protected override void OnCursorUp(Point cursorPosition)
		{
			base.OnCursorUp(cursorPosition);
			Log.Info($"Cursor up, {cursorPosition}");
			//_browserRenderer.HandleMouseUp(cursorPosition.X, cursorPosition.Y, MouseButton.Left);
		}

		/// <inheritdoc />
		protected override void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
		{
			base.OnCursorMove(cursorPosition, previousCursorPosition, isCursorDown);
			//_browserRenderer.HandleMouseMove(cursorPosition.X, cursorPosition.Y);
			
			Log.Info($"Cursor move, {cursorPosition}");
		}

		/// <inheritdoc />
		protected override void OnCursorPressed(Point cursorPosition, RocketUI.Input.MouseButton button)
		{
		//	Log.Info($"Cursor pressed: {cursorPosition}");
			base.OnCursorPressed(cursorPosition, button);
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

			var frame = _browserRenderer.CurrentFrame;
			if (frame != null)
			{
				graphics.SpriteBatch.Draw(frame, RenderBounds, frame.Bounds, Color.White);
			}
			//graphics.FillRectangle(RenderBounds, _webViewTexture, TextureRepeatMode.Stretch);
		}


		private MouseState _lastMouseState;
		private KeyboardHandler KeyHandler = new KeyboardHandler();
		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			var size = RenderSize;

			if (_previousSize != size)
			{
				size = GuiRenderer.Project(new Vector2(size.Width, size.Height)).ToPoint();
				_browserRenderer.Resize(new Size(size.Width, size.Height));
				_previousSize = size;
			}

			var mouseState = Mouse.GetState();

			var mousePosition = mouseState.Position;
			//if (mousePosition != _lastMouseState.Position)
			//{
				_browserRenderer.HandleMouseMove(mousePosition.X, mousePosition.Y);
			//}

			if (mouseState != _lastMouseState)
			{
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

				if (mouseState.ScrollWheelValue != _lastMouseState.ScrollWheelValue)
				{
					//if (mouse.RightButton == ButtonState.Pressed)
					//	_browserRenderer.HandleMouseDown(mousePosition.X, mousePosition.Y, MouseButton.Right);
					//else
					//	_browserRenderer.HandleMouseUp(mousePosition.X, mousePosition.Y, MouseButton.Right);
				}

				_lastMouseState = mouseState;
			}
			/*	KeyHandler.Update();
				var keys = KeyHandler.Query();
				foreach (var key in keys)
				{
					_browserRenderer.HandleKeyEvent(key);
				}*/

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
				AsyncHelpers.RunSync(
					async () => _browserRenderer.LoadPageAsync(str));
			}
		}
	}
}