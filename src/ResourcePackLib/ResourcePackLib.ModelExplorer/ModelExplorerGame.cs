using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ResourcePackLib.ModelExplorer.Abstractions;
using ResourcePackLib.ModelExplorer.Configuration;
using ResourcePackLib.ModelExplorer.Graphics;
using ResourcePackLib.ModelExplorer.Scenes;
using ResourcePackLib.ModelExplorer.Scenes.Screens;
using ResourcePackLib.ModelExplorer.Utilities.Extensions;
using RocketUI;
using RocketUI.Input;
using RocketUI.Input.Listeners;
using RocketUI.Utilities.Helpers;

namespace ResourcePackLib.ModelExplorer;

public class ModelExplorerGame : Game, IGame
{
    public IServiceProvider ServiceProvider { get; }
    public Game Game => this;
    public static IGame Instance { get; private set; }
    public GraphicsDeviceManager DeviceManager => _graphics;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public SceneManager SceneManager { get; private set; }
    public InputManager InputManager { get; private set; }

    public GuiManager GuiManager { get; private set; }
    
    public bool IsWireFrame { get; set; }

    public ModelExplorerGame(IServiceProvider services)
    {
        Instance = this;
        ServiceProvider = services;
        _graphics = new GraphicsDeviceManager(this)
        {
            SynchronizeWithVerticalRetrace = true,
            GraphicsProfile = GraphicsProfile.HiDef,
            PreferHalfPixelOffset = false,
            PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8,
            PreferredBackBufferWidth = Window.ClientBounds.Width,
            PreferredBackBufferHeight = Window.ClientBounds.Height
        };
        _graphics.PreparingDeviceSettings += OnGraphicsManagerOnPreparingDeviceSettings;
        IsFixedTimeStep = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowOnClientSizeChanged;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        var hostApplicationLifetime = services.GetRequiredService<IHostApplicationLifetime>();
        hostApplicationLifetime.ApplicationStopping.Register(OnHostApplicationStopping);
    }

    private void OnWindowOnClientSizeChanged(object? sender, EventArgs e)
    {
        if (DeviceManager.PreferredBackBufferWidth != Window.ClientBounds.Width ||
            DeviceManager.PreferredBackBufferHeight != Window.ClientBounds.Height)
        {
            if (DeviceManager.IsFullScreen)
            {
                DeviceManager.PreferredBackBufferWidth = DeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
                DeviceManager.PreferredBackBufferHeight = DeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            }
            else
            {
                DeviceManager.PreferredBackBufferWidth = Window.ClientBounds.Width;
                DeviceManager.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }
            DeviceManager.ApplyChanges();
        }
    }

    private void OnGraphicsManagerOnPreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
    {
        e.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.Depth24Stencil8;

        DeviceManager.PreferredBackBufferFormat = SurfaceFormat.Color;
        //  DeviceManager.PreferMultiSampling = true;

        DeviceManager.PreferredBackBufferWidth = Window.ClientBounds.Width;
        DeviceManager.PreferredBackBufferHeight = Window.ClientBounds.Height;
    }

    private void OnHostApplicationStopping()
    {
        GpuResourceManager.Dispose();
    }
    protected override void Initialize()
    {
        DeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
        GraphicsDevice.PresentationParameters.MultiSampleCount = 8;
        DeviceManager.SynchronizeWithVerticalRetrace = true;
//            GraphicsDeviceManager.PreferMultiSampling = true;
        IsFixedTimeStep = true;
        Window.AllowUserResizing = true;
            
        DeviceManager.ApplyChanges();
        
        Components.Add(InputManager = ServiceProvider.GetRequiredService<InputManager>());
        Components.Add(SceneManager = ServiceProvider.GetRequiredService<SceneManager>());
        Components.Add(GuiManager = ServiceProvider.GetRequiredService<GuiManager>());
        
        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        GpuResourceManager.Init(GraphicsDevice);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        base.LoadContent();
        _camera = new Camera(this)
        {
            
        };
        Components.Add(_camera);
        Camera = _camera;
        Camera.Position = new Vector3(1.7f, 1.7f, 1.7f);
        // cam.Rotation = Quaternion.CreateFromYawPitchRoll(270f, (float)Math.PI / 2f, 0f);
            
        
        GuiManager.ScaledResolution.TargetWidth = 480;
        GuiManager.ScaledResolution.TargetHeight = 320;
        GuiManager.AddScreen(_debugGui = new DebugGui());
        GuiManager.DrawOrder = 10;
        GuiManager.Init();
        
        Options = ServiceProvider.GetRequiredService<IOptions<GameOptions>>().Value;
        
        SceneManager.SetScene<MainMenuScene>();
        _graphics.GraphicsDevice.Viewport = new Viewport(Window.ClientBounds);
        GuiManager.Reinitialize();

        InputManager.GetOrAddPlayerManager(PlayerIndex.One).TryGetListener<KeyboardInputListener>(out _keyboardListener);

    }
    public GameOptions Options { get; set; }
    private DebugGui _debugGui;
    private Camera _camera;
    private KeyboardInputListener _keyboardListener;

    protected override void Update(GameTime gameTime)
    {
        // if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
        //     Keyboard.GetState().IsKeyDown(Keys.Escape))
        //     Exit();

        if (_keyboardListener.IsAnyPressed(Keys.F8))
        {
            IsWireFrame = !IsWireFrame;
        }

        base.Update(gameTime);
    }
    public ICamera Camera { get; private set; }
    
    protected override void Draw(GameTime gameTime)
    {
        Camera?.Draw(() => base.Draw(gameTime));
    }
}