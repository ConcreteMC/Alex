using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Alex.Common.Services;
using Alex.Utils;
using ImpromptuNinjas.UltralightSharp;
using ImpromptuNinjas.UltralightSharp.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SixLabors.ImageSharp.PixelFormats;
using App = ImpromptuNinjas.UltralightSharp.Safe.App;
using String = ImpromptuNinjas.UltralightSharp.String;
using Ultralight = ImpromptuNinjas.UltralightSharp.Safe.Ultralight;

namespace Alex.Gui
{
	public unsafe class HtmlView : IDisposable
	{
		private View* _view;
		public HtmlView(View* view)
		{
			_view = view;
		}

		public Uri Url
		{
			get
			{
				return new Uri(_view->GetUrl()->Read());
			}
			set
			{
				var str = String.Create(value.ToString());
				_view->LoadUrl(str);
				str->Destroy();
			}
		}

		private bool _disposed = false;
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_disposed)
					return;
				
				_disposed = true;
			}

			if (_view != null)
			{
				_view->Destroy();
				_view = null;
			}
		}
		
		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
		}

		~HtmlView()
		{
			Dispose(false);
		}
	}
	public unsafe class HtmlRenderer : DrawableGameComponent
	{
		private readonly IStorageSystem _storage;
		private readonly IOptionsProvider _options;
		private Renderer* _renderer = null;
		private Session* _session = null;
		private View* _view = null;
		private Config* _config = null;

		private Texture2D _texture = null;
		private SpriteBatch _spriteBatch = null;

		private List<HtmlView> _views = new List<HtmlView>();
		/// <inheritdoc />
		public HtmlRenderer(Alex game, IStorageSystem storage, IOptionsProvider options) : base(game)
		{
			_storage = storage;
			_options = options;
		}

		/// <inheritdoc />
		public override void Initialize()
		{
			base.Initialize();
			
			
		}

		/// <inheritdoc />
		protected override void LoadContent()
		{
			base.LoadContent();
			
			_spriteBatch = new SpriteBatch(Game.GraphicsDevice);
			
			var asmPath = new Uri(typeof(Alex).Assembly.CodeBase!).LocalPath;
			var asmDir = Path.GetDirectoryName(asmPath)!;
			var tempDir = Path.GetTempPath();
			// find a place to stash instance storage
			string storagePath;
			do {
				storagePath = Path.Combine(tempDir, Guid.NewGuid().ToString());
			} while (Directory.Exists(storagePath) || File.Exists(storagePath));
			
			var cfg = Config.Create();
			{
				var cachePath = String.Create(Path.Combine(storagePath, "Cache"));
				cfg->SetCachePath(cachePath);
				cachePath->Destroy();
			}

			{
				var resourcePath = String.Create(Path.Combine(asmDir, "resources"));
				cfg->SetResourcePath(resourcePath);
				resourcePath->Destroy();
			}
			cfg->SetUseGpuRenderer(false);
			cfg->SetEnableImages(true);
			cfg->SetEnableJavaScript(true);
			AppCore.EnablePlatformFontLoader();
			{
				var assetsPath = String.Create(Path.Combine(asmDir, "assets"));
				AppCore.EnablePlatformFileSystem(assetsPath);
				assetsPath->Destroy();
			}
			
			var renderer = Renderer.Create(cfg);
			var sessionName = String.Create("AlexGUI");
			var session = Session.Create(renderer, false, sessionName);

			var view = View.Create(renderer, (uint)Game.Window.ClientBounds.Width, (uint)Game.Window.ClientBounds.Height, true, session);
			view->Focus();
			{
				var htmlString = String.Create("<i>Loading...</i>");
				Console.WriteLine($"Loading HTML: {htmlString->Read()}");
				view->LoadHtml(htmlString);
				htmlString->Destroy();
			}

			_config = cfg;
			_renderer = renderer;
			_session = session;
			_view = view;
			
			Game.Window.ClientSizeChanged += WindowOnClientSizeChanged;
			Game.Window.KeyUp += WindowOnKeyUp;
			Game.Window.KeyDown += WindowOnKeyDown;
			Game.Window.TextInput += WindowOnTextInput;
			
			var loaded = false;

			view->SetFinishLoadingCallback((data, caller, frameId, isMainFrame, url) => {
				Console.WriteLine($"Loading Finished, URL: 0x{(ulong) url:X8}  {url->Read()}");

				loaded = true;
			}, null);
			
			var urlString = String.Create("http://google.com");
			Console.WriteLine($"Loading URL: {urlString->Read()}");
			view->LoadUrl(urlString);
			urlString->Destroy();
			
			UpdateTexture();
		}
		
		private void UpdateTexture()
		{
			var w = Game.Window.ClientBounds.Width;
			var h = Game.Window.ClientBounds.Height;
			
			_view->Resize((uint)w, (uint)h);
			var texture = new Texture2D(Game.GraphicsDevice, w, h);
			var oldTexture = _texture;

			_texture = texture;
			
			if (oldTexture != null)
			{
				oldTexture.Dispose();
			}
		}

		private void WindowOnClientSizeChanged(object? sender, EventArgs e)
		{
			UpdateTexture();
		}

		/// <inheritdoc />
		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
			
			if (_renderer == null || _spriteBatch == null)
				return;

			_renderer->Render();
			_spriteBatch.Begin();
			_spriteBatch.Draw(_texture, new Rectangle(0, 0, (int)_texture.Width, (int)_texture.Height), Color.White);
			_spriteBatch.End();
		}
		
	//	private void FireMouseEvent(MouseState stat)

		private MouseState _mouseState = new MouseState();
		/// <inheritdoc />
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (_renderer == null)
				return;

			_renderer->Update();

			var mouseState = Mouse.GetState();

			if (mouseState != _mouseState)
			{
				if (_mouseState.LeftButton != mouseState.LeftButton)
				{
					if (mouseState.LeftButton == ButtonState.Pressed)
					{
						_view->FireMouseEvent(MouseEvent.Create(MouseEventType.MouseDown, mouseState.X, mouseState.Y, MouseButton.Left));
					}
					else if (mouseState.LeftButton == ButtonState.Released)
					{
						_view->FireMouseEvent(MouseEvent.Create(MouseEventType.MouseUp, mouseState.X, mouseState.Y, MouseButton.Left));
					}
				}
				
				if (_mouseState.RightButton != mouseState.RightButton)
				{
					if (mouseState.RightButton == ButtonState.Pressed)
					{
						_view->FireMouseEvent(MouseEvent.Create(MouseEventType.MouseDown, mouseState.X, mouseState.Y, MouseButton.Right));
					}
					else if (mouseState.RightButton == ButtonState.Released)
					{
						_view->FireMouseEvent(MouseEvent.Create(MouseEventType.MouseUp, mouseState.X, mouseState.Y, MouseButton.Right));
					}
				}
				
				if (_mouseState.MiddleButton != mouseState.MiddleButton)
				{
					if (mouseState.MiddleButton == ButtonState.Pressed)
					{
						_view->FireMouseEvent(MouseEvent.Create(MouseEventType.MouseDown, mouseState.X, mouseState.Y, MouseButton.Middle));
					}
					else if (mouseState.MiddleButton == ButtonState.Released)
					{
						_view->FireMouseEvent(MouseEvent.Create(MouseEventType.MouseUp, mouseState.X, mouseState.Y, MouseButton.Middle));
					}
				}

				if (_mouseState.Position != mouseState.Position)
				{
					_view->FireMouseEvent(MouseEvent.Create(MouseEventType.MouseMoved, mouseState.X, mouseState.Y, MouseButton.None));
				}

				var xDelta = mouseState.HorizontalScrollWheelValue - _mouseState.HorizontalScrollWheelValue;
				var yDelta = mouseState.ScrollWheelValue - _mouseState.ScrollWheelValue;

				if (xDelta != 0 || yDelta != 0)
				{
					_view->FireScrollEvent(ScrollEvent.Create(ScrollEventType.ScrollByPixel, xDelta, yDelta));
				}
			}

			_mouseState = mouseState;

			var surface = _view->GetSurface();
			var dirtyBounds = surface->GetDirtyBounds();

			if (!dirtyBounds.IsEmpty())
			{
				var bitmap = surface->GetBitmap();

				var pixels = bitmap->LockPixels();

				var w = bitmap->GetWidth();
				var h = bitmap->GetHeight();

				try
				{
					var pPixels = (byte*)pixels;
					var span = new ReadOnlySpan<Bgra32>(pPixels, checked((int)(w * h)));
					_texture.SetData(span.ToArray().Select(x => x.PackedValue).ToArray());
				}
				finally
				{
					bitmap->UnlockPixels();
					//	bitmap->SwapRedBlueChannels();
					surface->ClearDirtyBounds();
				}
			}
		}

		private void WindowOnTextInput(object? sender, TextInputEventArgs e)
		{
			if (_view == null)
				return;
			
			if (char.IsLetterOrDigit(e.Character) || char.IsPunctuation(e.Character) || char.IsSymbol(e.Character) || char.IsWhiteSpace(e.Character))
			{
				var str = String.Create(e.Character.ToString());

				_view->FireKeyEvent(
					KeyEvent.Create(KeyEventType.Char, 0, (int)e.Key, (int)e.Key, str, str, false, false, false));

				str->Destroy();
				
				//GuiManager.FocusManager.OnTextInput(this, e);
			}
		}

		
		private void WindowOnKeyDown(object? sender, InputKeyEventArgs e)
		{
			if (_view == null)
				return;

			if (!e.Key.TryConvertKeyboardInput(out _))
			{
				var str = String.Create(e.Key.ToString());

				_view->FireKeyEvent(
					KeyEvent.Create(KeyEventType.KeyDown, 0, (int)e.Key, (int)e.Key, str, str, false, false, false));

				str->Destroy();
			}
			//_view->FireKeyEvent(KeyEvent.Create(KeyEventType.KeyDown, e.))
		}

		private void WindowOnKeyUp(object? sender, InputKeyEventArgs e)
		{
			if (_view == null)
				return;
			
			if (!e.Key.TryConvertKeyboardInput(out _))
			{
				var str = String.Create(e.Key.ToString());

				_view->FireKeyEvent(
					KeyEvent.Create(KeyEventType.KeyUp, 0, (int)e.Key, (int)e.Key, str, str, false, false, false));

				str->Destroy();
			}
		}

		private bool _disposed = false;
		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_disposed)
				return;

			_disposed = true;

			_view->Destroy();
			_view = null;

			_session->Destroy();
			_session = null;
			
			_renderer->Destroy();
			_renderer = null;

			_config->Destroy();
			_config = null;
			
			_spriteBatch.Dispose();
			_spriteBatch = null;
		}
		

		~HtmlRenderer()
		{
			Dispose(false);
		}
	}
}