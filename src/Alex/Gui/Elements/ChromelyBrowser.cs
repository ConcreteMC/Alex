using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Alex.API.Input;
using CefSharp;
using CefSharp.Enums;
using CefSharp.Handler;
using CefSharp.Internals;
using CefSharp.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WinApi.Gdi32;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace Alex.Gui.Elements
{
	public class BoundObject
	{
		public void WriteCursorCodes(string test = "")
		{
		}
	}

	public class ChromiumWebBrowser : IRenderWebBrowser
	{
		public readonly object BitmapLock = new object();
		public Texture2D MainWindow;
		public Texture2D Popup;

		private ManagedCefBrowserAdapter _managedCefBrowserAdapter;
		private Size _size = new Size(1366, 768);
		private BoundObject _bound = new BoundObject();
		private GraphicsDevice _graphicsDevice;

		public ChromiumWebBrowser(GraphicsDevice graphicsDevice, string address = "",
			BrowserSettings browserSettings = null, RequestContext context = null)
		{
		//	if (!Cef.IsInitialized && !Cef.Initialize())
		//		throw new InvalidOperationException("Cef::Initialize() failed");

			ResourceHandlerFactory = new DefaultResourceHandlerFactory();
			BrowserSettings = browserSettings ?? new BrowserSettings();
			RequestContext = context ?? new RequestContext();
			Cef.AddDisposable(this);

			Address = address;
			RequestHandler = new DefaultRequestHandler();
			
			_managedCefBrowserAdapter = new ManagedCefBrowserAdapter(this, true);
			_managedCefBrowserAdapter.CreateOffscreenBrowser(IntPtr.Zero, BrowserSettings, (RequestContext)RequestContext, address);
			//managedCefBrowserAdapter.SetFocus(true);
			_graphicsDevice = graphicsDevice;
			//            RegisterJsObject("bound", bound);
		}

		public BrowserSettings BrowserSettings { get; private set; }

		public Size Size
		{
			get { return _size; }
			set
			{
				if (_size == value) return;
				_size = value;
				_managedCefBrowserAdapter.Resize(value.Width, value.Height);
			}
		}

		public IFindHandler FindHandler { get; set; }
		public bool IsBrowserInitialized { get; private set; }
		public bool IsLoading { get; set; }
		public string Title { get; set; }
		public string TooltipText { get; set; }
		public bool CanExecuteJavascriptInMainFrame { get; private set; }
		public IRequestContext RequestContext { get; }
		public string Address { get; private set; }
		public bool CanReload { get; private set; }
		public bool CanGoBack { get; private set; }
		public bool CanGoForward { get; private set; }
		public IJsDialogHandler JsDialogHandler { get; set; }
		public IDialogHandler DialogHandler { get; set; }
		public IDownloadHandler DownloadHandler { get; set; }

		public IKeyboardHandler KeyboardHandler { get; set; }
		public ILoadHandler LoadHandler { get; set; }
		public ILifeSpanHandler LifeSpanHandler { get; set; }
		public IContextMenuHandler MenuHandler { get; set; }
		public IFocusHandler FocusHandler { get; set; }
		public IRequestHandler RequestHandler { get; set; }
		public IDisplayHandler DisplayHandler { get; set; }
		public IDragHandler DragHandler { get; set; }
		public IResourceHandlerFactory ResourceHandlerFactory { get; set; }
		public IRenderProcessMessageHandler RenderProcessMessageHandler { get; set; }
		//public IGeolocationHandler GeolocationHandler { get; set; }
		public event EventHandler<LoadErrorEventArgs> LoadError;
		public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;
		public event EventHandler<FrameLoadStartEventArgs> FrameLoadStart;
		public event EventHandler<FrameLoadEndEventArgs> FrameLoadEnd;
		public event EventHandler<ConsoleMessageEventArgs> ConsoleMessage;
		public event EventHandler<StatusMessageEventArgs> StatusMessage;
		//public event EventHandler<NavStateChangedEventArgs> NavStateChanged;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void ShowDevTools()
		{
			GetBrowser().ShowDevTools();
		}

		public void CloseDevTools()
		{
			GetBrowser().CloseDevTools();
		}

		public void ReplaceMisspelling(string word)
		{
			GetBrowser().ReplaceMisspelling(word);
		}

		public void AddWordToDictionary(string word)
		{
			GetBrowser().AddWordToDictionary(word);
		}

		public Task<JavascriptResponse> EvaluateScriptAsync(string script, TimeSpan? timeout = null)
		{
			return GetBrowser().MainFrame.EvaluateScriptAsync(script, timeout: timeout);
		}

		public void ExecuteScriptAsync(string script)
		{
			GetBrowser().MainFrame.ExecuteJavaScriptAsync(script);
		}

		public void Find(int identifier, string searchText, bool forward, bool matchCase, bool findNext)
		{
			GetBrowser().Find(identifier, searchText, forward, matchCase, findNext);
		}

		public void StopFinding(bool clearSelection)
		{
			GetBrowser().StopFinding(clearSelection);
		}

		public void Load(string url)
		{
			Address = url;
			GetBrowser().MainFrame.LoadUrl(Address);
		}

		public void RegisterJsObject(string name, object objectToBind, BindingOptions options = null)
		{
			throw new NotImplementedException();
		}

		public void RegisterAsyncJsObject(string name, object objectToBind, BindingOptions options = null)
		{
			throw new NotImplementedException();
		}

		public void LoadHtml(string html, string url)
		{
			LoadHtml(html, url, Encoding.UTF8);
		}

		public void LoadHtml(string html, string url, Encoding encoding)
		{
			DefaultResourceHandlerFactory factory = ResourceHandlerFactory as DefaultResourceHandlerFactory;
			if (factory == null)
				throw new Exception(
					"Implement IResourceHandlerFactory and assign to the ResourceHandlerFactory property to use this feature");
			factory.RegisterHandler(url, ResourceHandler.FromString(html, encoding, true));
			Load(url);
		}

		public void RegisterJsObject(string name, object objectToBind, bool camelCaseJavascriptNames = true)
		{
			_managedCefBrowserAdapter.JavascriptObjectRepository.Register(name, objectToBind, false, new BindingOptions()
			{
				CamelCaseJavascriptNames = camelCaseJavascriptNames
			});
		}


		public Task<string> GetSourceAsync()
		{
			var taskStringVisitor = new TaskStringVisitor();
			GetBrowser().MainFrame.GetSource(taskStringVisitor);//.GetSource(taskStringVisitor);
			return taskStringVisitor.Task;
		}

		public Task<string> GetTextAsync()
		{
			var taskStringVisitor = new TaskStringVisitor();
			GetBrowser().MainFrame.GetText(taskStringVisitor);
			return taskStringVisitor.Task;
		}

		bool IWebBrowser.Focus()
		{
			return true;
		}

		public IBrowser GetBrowser()
		{
			throw new NotImplementedException();
		}

		public IJavascriptObjectRepository JavascriptObjectRepository { get; }

		public void Reload()
		{
			Reload(false);
		}

		public void Reload(bool ignoreCache)
		{
			GetBrowser().Reload(ignoreCache);
		}

		public void ViewSource()
		{
			GetBrowser().MainFrame.ViewSource();
		}

		public void Print()
		{
			GetBrowser().Print();
		}

		public void Back()
		{
			GetBrowser().GoBack();
		}

		public void Forward()
		{
			GetBrowser().GoForward();
		}

		public event EventHandler BrowserInitialized;
		public event EventHandler<AddressChangedEventArgs> AddressChanged;

		~ChromiumWebBrowser()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			// Don't reference handlers any longer:
			ResourceHandlerFactory = null;
			JsDialogHandler = null;
			DialogHandler = null;
			DownloadHandler = null;
			KeyboardHandler = null;
			LifeSpanHandler = null;
			MenuHandler = null;
			FocusHandler = null;
			RequestHandler = null;
			DragHandler = null;

			// Don't reference event listeners any longer:
			LoadError = null;
			FrameLoadStart = null;
			FrameLoadEnd = null;
			ConsoleMessage = null;
			BrowserInitialized = null;
			StatusMessage = null;
			AddressChanged = null;

			Cef.RemoveDisposable(this);

			if (!disposing) return;
			IsBrowserInitialized = false;

			if (BrowserSettings != null)
			{
				BrowserSettings.Dispose();
				BrowserSettings = null;
			}

			if (_managedCefBrowserAdapter == null) return;
			if (!_managedCefBrowserAdapter.IsDisposed)
			{
				_managedCefBrowserAdapter.Dispose();
			}

			_managedCefBrowserAdapter = null;
		}

		/*public void OnMouseMove(Point position, CefEventFlags modifiers)
		{
			if (_managedCefBrowserAdapter != null)
			{
				_managedCefBrowserAdapter.OnMouseMove((int) position.X, (int) position.Y, false, modifiers);
			}
		}

		public void OnMouseButton(bool validButton, Point position, int changedButton, bool mouseUp, int clickCount,
			CefEventFlags modifiers)
		{
			// Cef currently only supports Left, Middle and Right button presses.
			if (!validButton) return;

			if (_managedCefBrowserAdapter != null)
			{
				_managedCefBrowserAdapter.OnMouseButton((int) position.X, (int) position.Y, changedButton, mouseUp,
					clickCount, modifiers);
			}
		}
		*/
		#region IRenderWebBrowser (rendering to bitmap; derived from CefSharp.Wpf.ChromiumWebBrowser)
		

		public Rect? GetViewRect()
		{
			throw new NotImplementedException();
		}

		public bool GetScreenPoint(int viewX, int viewY, out int screenX, out int screenY)
		{
			throw new NotImplementedException();
		}

		public void OnPaint(PaintElementType type, Rect dirtyRect, IntPtr buffer, int width, int height)
		{
			throw new NotImplementedException();
		}

		public void OnCursorChange(IntPtr cursor, CursorType type, CursorInfo customCursorInfo)
		{
			throw new NotImplementedException();
		}

		public bool StartDragging(IDragData dragData, DragOperationsMask mask, int x, int y)
		{
			throw new NotImplementedException();
		}

		public void UpdateDragCursor(DragOperationsMask operation)
		{
			throw new NotImplementedException();
		}

		public void OnPopupShow(bool show)
		{
			throw new NotImplementedException();
		}

		public void OnPopupSize(Rect rect)
		{
			throw new NotImplementedException();
		}

		public void OnImeCompositionRangeChanged(Range selectedRange, Rect[] characterBounds)
		{
			throw new NotImplementedException();
		}

		public IAccessibilityHandler AccessibilityHandler { get; set; }



		#endregion

		private IBrowser ActiveBrowser;
		#region IWebBrowserInternal (notifications from CEF to C#; derived from CefSharp.Wpf.ChromiumWebBrowser)

		public void OnAfterBrowserCreated(IBrowser browser)
		{
			ActiveBrowser = browser;
			//	browser.CloseBrowser();
		}

		public void SetAddress(AddressChangedEventArgs args)
		{
			Address = args.Address;
			AddressChanged?.Invoke(this, args);
		}

		public void SetLoadingStateChange(LoadingStateChangedEventArgs args)
		{
			CanGoBack = args.CanGoBack;
			CanGoForward = args.CanGoForward;
			CanReload = !args.IsLoading;
			this.LoadingStateChanged?.Invoke(this, args);
		}

		public void SetTitle(TitleChangedEventArgs args)
		{
			Title = args.Title;
		}

		void IWebBrowserInternal.SetTooltipText(string tooltipText)
		{
			TooltipText = tooltipText;
		}

		public void SetCanExecuteJavascriptOnMainFrame(bool canExecute)
		{
			CanExecuteJavascriptInMainFrame = canExecute;
		}

		public void OnFrameLoadStart(FrameLoadStartEventArgs args)
		{
			FrameLoadStart?.Invoke(this, args);
		}

		public void OnFrameLoadEnd(FrameLoadEndEventArgs args)
		{
			FrameLoadEnd?.Invoke(this, args);
		}

		public void OnConsoleMessage(ConsoleMessageEventArgs args)
		{
			throw new NotImplementedException();
		}

		public void OnStatusMessage(StatusMessageEventArgs args)
		{
			throw new NotImplementedException();
		}

		public void OnLoadError(LoadErrorEventArgs args)
		{
			throw new NotImplementedException();
		}

		public IBrowserAdapter BrowserAdapter => _managedCefBrowserAdapter;
		public bool HasParent { get; set; }

		#endregion

		public ScreenInfo? GetScreenInfo()
		{
			var screenInfo = new ScreenInfo(); //{ Width = _size.Width, Height = _size.Height, ScaleFactor = 1.0F };
			return screenInfo;
		}
	}
}
