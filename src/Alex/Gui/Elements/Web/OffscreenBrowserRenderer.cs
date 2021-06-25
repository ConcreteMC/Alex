using System;
using System.Threading.Tasks;
using Alex.Common.Graphics.GpuResources;
using Alex.Common.Utils;
using Alex.Gui.Dialogs;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NLog;
using PuppeteerSharp;
using PuppeteerSharp.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;
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
    public EventHandler<Uri> OnNavigate;

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
    public async void Resize(System.Drawing.Size size)
    {
      if (_page != null)
      {
        await _page.SetViewportAsync(
          new ViewPortOptions()
          {
            Height = size.Height, 
            Width = size.Width, 
            DeviceScaleFactor = 1f
          });
      }
      // Browser.Size = size;
    }

    public void Update()
    {
     // if (_frameDirty)
      {
      //  _frameDirty = false;
        UpdateFrame(_page);
      }
    }
    
    private async void UpdateFrame(Page page)
    {
      var bmpData = await page.ScreenshotDataAsync(new ScreenshotOptions()
      {
        OmitBackground = true,
        
      });
      
      {
       // if (CurrentFrame == null)
        {
          CurrentFrame = TextureUtils.ImageToTexture2D(this, Alex.Instance.GraphicsDevice, bmpData);
         // CurrentFrame = GpuResourceManager.Get(this, Alex.Instance.GraphicsDevice);
        }
       // else
        {
        //  Image<Rgba32> img = Image.Load(bmpData);
//
        //  if (img.TryGetSinglePixelSpan(out var screenData))
        //  {
        //    CurrentFrame.SetData(screenData.ToArray().Select(x => new Color(x.PackedValue)).ToArray());
         // }
        }
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
      await page.ExposeFunctionAsync("reportCursor", (string cursor) =>
      {
        switch (cursor)
        {
          case "pointer":
            Mouse.SetCursor(MouseCursor.Hand);
            break;
          case "text":
            Mouse.SetCursor(MouseCursor.IBeam);
            break;
          default:
            //Log.Info($"Got cursor: {cursor}");
            Mouse.SetCursor(MouseCursor.Arrow);
            break;
        }
          return false;
      });
      
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

        document.addEventListener('mouseover',function(e) {{
         const tgt = e.target;
         const inline = tgt.style.cursor || 'Not defined';
         const computed = window.getComputedStyle(tgt)['cursor'];
         reportCursor(computed);
        }}, false);

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
        //Log.Info($"Navigated: {e.Frame.Url}");
        OnNavigate?.Invoke(this, new Uri(e.Frame.Url));
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

    public async void HandleMouseScroll(int deltaX, int deltaY)
    {
      if (Browser != null)
      {
          await _page.Mouse.WheelAsync(deltaX, deltaY);
      }
    }
    
    public async void HandleMouseMove(float x, float y)
    {
      if (Browser != null)
      {
        await _page.Mouse.MoveAsync((decimal) x, (decimal) y);
      }
    }
    public async void HandleMouseDown(float x, float y, MouseButton type)
    {
      if (Browser != null)
      {
        await _page.Mouse.MoveAsync((decimal) x, (decimal) y);
        await _page.Mouse.DownAsync(new ClickOptions() {Button = type});
      }
    }
    public async void HandleMouseUp(float x, float y, MouseButton type)
    {
      if (Browser != null)
      {
        await _page.Mouse.MoveAsync((decimal) x, (decimal) y);
        await _page.Mouse.UpAsync(new ClickOptions() {Button = type});
       // await _page.Mouse.ClickAsync(x, y, new ClickOptions() {Button = type});
      }
    }

    public async void OnKeyInput(char character, Keys key)
    {
      if (!char.IsControl(character))
      {
          await _page.Keyboard.SendCharacterAsync(character.ToString());
      }
      else
      {
        if (key == Keys.Back)
        {
          await _page.Keyboard.PressAsync("Backspace");
        }
        else if (key == Keys.Delete)
        {
          await _page.Keyboard.PressAsync("Delete");
        }
        else if (key == Keys.Left)
        {
          await _page.Keyboard.PressAsync("ArrowLeft");
        }
        else if (key == Keys.Right)
        {
          await _page.Keyboard.PressAsync("ArrowRight");
        }
        else if (key == Keys.Up)
        {
          await _page.Keyboard.PressAsync("ArrowUp");
        }
        else if (key == Keys.Down)
        {
          await _page.Keyboard.PressAsync("ArrowDown");
        }
        else if (key == Keys.Enter)
        {
          await _page.Keyboard.PressAsync("Enter");
        }
       // if (KeyDefinitions.TryGet(key, out var keyDefinition))
       // {
          //await _page.Keyboard.PressAsync(keyDefinition.Code);
       // }
      }
    }
  }
}