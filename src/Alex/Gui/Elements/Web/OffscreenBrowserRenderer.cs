using System;
using System.Threading.Tasks;
using Alex.Common.Graphics.GpuResources;
using Alex.Utils;
using Microsoft.Xna.Framework.Graphics;
using NLog;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseButton = PuppeteerSharp.Input.MouseButton;

namespace Alex.Gui.Elements.Web
{
  public class OffscreenBrowserRenderer : IDisposable
  {
    public OffscreenBrowserRenderer()
    {
      AsyncHelpers.RunSync(
        () =>
        {
          var browserFetcher = new BrowserFetcher();
          return browserFetcher.DownloadAsync();
        });
    }

    public void Dispose()
    {
     // Cef.Shutdown();
    }

    public Browser Browser;

    public ManagedTexture2D CurrentFrame
    {
      get => _currentFrame;
      private set
      {
        var oldFrame = _currentFrame;

        if (oldFrame == value) return;
        
        _currentFrame = value;

        if (oldFrame != null)
        {
          oldFrame.ReturnResource(this);
        }
      }
    }

    private bool _frameDirty = false;
   // private bool _ready = false;
    public void Resize(System.Drawing.Size size)
    {
      if (_page != null)
      {
        AsyncHelpers.RunSync(
          async () =>
          {
            await _page.SetViewportAsync(new ViewPortOptions() {Height = size.Height, Width = size.Width});

          //  _ready = true;
          });
      }
      // Browser.Size = size;
    }

    public void Update()
    {
      if (_frameDirty)
      {
        _frameDirty = false;
        UpdateFrame(_page);
      }
    }
    
    private void UpdateFrame(Page page)
    {
      using (var bmp = AsyncHelpers.RunSync(
        () => page.ScreenshotStreamAsync(new ScreenshotOptions())))
      {
        CurrentFrame = GpuResourceManager.GetTexture2D(this, Alex.Instance.GraphicsDevice, bmp);
      }
    }
    
    private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(OffscreenBrowserRenderer));
    public async Task MainAsync(GraphicsDevice gd, IntPtr windowHandle, string url, object data, System.Drawing.Size size, double zoomLevel = 1.0)
    {
      Browser = await Puppeteer.LaunchAsync(
        new LaunchOptions { Headless = true, DefaultViewport = new ViewPortOptions()
        {
           Height = size.Height,
           Width = size.Width
        }});
      
      var page = await Browser.NewPageAsync();
      await page.SetJavaScriptEnabledAsync(true);
      
      await page.ExposeFunctionAsync("alexDomChanged", InvalidateView);
      
      page.Console += PageOnConsole;
      page.FrameNavigated += PageOnFrameNavigated;
      
      _page = page;
      Resize(size);
      
      await LoadPageAsync(url);
    }
    
    public async Task LoadPageAsync(string address = null)
    {
      // var tcs = new TaskCompletionSource<bool>();
     if (_page != null)
     {
       var response = await _page.GoToAsync(address, WaitUntilNavigation.DOMContentLoaded);
       await Bind(_page);
     }
    }

    private async Task Bind(Page page)
    {
      try
      {
        await page.EvaluateFunctionAsync(
          @"() => {

        if (!document.body)
          return;

        var observer = new MutationObserver((mutations) => {
        alexDomChanged();
      });
      observer.observe(document.body, {
        attributes: true,
				characterData: true,
				childList: true,
        subtree: true,
      });

        document.addEventListener('focus',function(e){
          alexDomChanged();
        }, true);

        }");
      }
      catch (Exception error)
      {
        Log.Warn(error, "Oh no. Failed to bind.");
      }
    }

    private string _previousUrl = null;

    private async void PageOnFrameNavigated(object? sender, FrameEventArgs e)
    {
      if (e.Frame.ParentFrame != null)
        return;

      //  if (!string.Equals(e.Frame.Url, ))
      if (e.Frame.Url != _previousUrl)
      {
        _previousUrl = e.Frame.Url;
        Log.Info($"Navigated: {e.Frame.Url}");

        await Bind(_page);
        // if (_page != null)
        // await Bind(_page);
      }
    }

    private void PageOnConsole(object? sender, ConsoleEventArgs e)
    {
      switch (e.Message.Type)
      {
        
      }
      Log.Info($"Log: {e.Message.Text}");
    }

    private void InvalidateView()
    {
      _frameDirty = true;
    }

    private Page _page;
    private ManagedTexture2D _currentFrame;
    
    public void HandleMouseMove(int x, int y)
    {
      if (Browser != null)
      {
        AsyncHelpers.RunSync(() => _page.Mouse.MoveAsync(x, y));
      }
    }
    public void HandleMouseDown(int x, int y, MouseButton type)
    {
      if (Browser != null)
      {
        AsyncHelpers.RunSync(() => _page.Mouse.ClickAsync(x, y, new ClickOptions(){Button = type}));
      }
    }
    public void HandleMouseUp(int x, int y, MouseButton type)
    {
      if (Browser != null)
      {
        AsyncHelpers.RunSync(() => _page.Mouse.ClickAsync(x, y, new ClickOptions(){Button = type}));
      }
    }

    public void OnKeyInput(char character, Keys key)
    {
      if (key.TryConvertKeyboardInput(out var keyCharacter))
      {
        AsyncHelpers.RunSync(() => _page.Keyboard.PressAsync(keyCharacter.ToString()));
      }
      else
      {
        AsyncHelpers.RunSync(() => _page.Keyboard.PressAsync(key.ToString()));
      }
    }
    
    public void HandleKeyEvent(KeyEvent k)
    {
      if (Browser != null)
      {
        var page = _page;
        switch (k.Type)
        {
          case KeyEventType.KeyUp:
            page.Keyboard.UpAsync(k.Key.ToString());
            break;

          case KeyEventType.KeyDown:
            page.Keyboard.DownAsync(k.Key.ToString(), new DownOptions()
            {
              
            });
            break;

          case KeyEventType.Char:
            page.Keyboard.SendCharacterAsync(k.Key.ToString());
            break;
        }
      }
    }
  }
}