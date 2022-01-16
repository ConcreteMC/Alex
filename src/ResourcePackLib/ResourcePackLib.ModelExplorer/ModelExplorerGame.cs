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
    public GraphicsDeviceManager GraphicsDeviceManager => _graphics;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public SceneManager SceneManager { get; private set; }
    public InputManager InputManager { get; private set; }

    public GuiManager GuiManager { get; private set; }

    public ModelExplorerGame(IServiceProvider services)
    {
        Instance = this;
        ServiceProvider = services;
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        var hostApplicationLifetime = services.GetRequiredService<IHostApplicationLifetime>();
        hostApplicationLifetime.ApplicationStopping.Register(OnHostApplicationStopping);
    }

    private void OnHostApplicationStopping()
    {
        GpuResourceManager.Dispose();
    }
    protected override void Initialize()
    {
        GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
        GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
//            GraphicsDeviceManager.PreferMultiSampling = true;
        IsFixedTimeStep = false;
        Window.AllowUserResizing = true;
            
        GraphicsDeviceManager.ApplyChanges();

        InputManager = ServiceProvider.GetRequiredService<InputManager>();
        Components.Add(InputManager);            
            
        Components.Add(SceneManager = ServiceProvider.GetRequiredService<SceneManager>());
        
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
            
        GuiManager = ServiceProvider.GetRequiredService<GuiManager>();
        GuiManager.ScaledResolution.TargetWidth = 720;
        GuiManager.ScaledResolution.TargetHeight = 360;
        GuiManager.AddScreen(_debugGui = new DebugGui());
        Components.Add(GuiManager);
        GuiManager.DrawOrder = 10;
        GuiManager.Init();
        
        Options = ServiceProvider.GetRequiredService<IOptions<GameOptions>>().Value;
        
        SceneManager.SetScene<MainMenuScene>();
        _graphics.GraphicsDevice.Viewport = new Viewport(Window.ClientBounds);

        InputManager.GetOrAddPlayerManager(PlayerIndex.One).TryGetListener<KeyboardInputListener>(out _keyboardListener);

    }
    public GameOptions Options { get; set; }
    private DebugGui _debugGui;
    private Camera _camera;
    private KeyboardInputListener _keyboardListener;

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (_keyboardListener.IsAnyPressed(Keys.F8))
        {
            var r = GraphicsDevice.RasterizerState.Copy();
            r.FillMode = (r.FillMode == FillMode.Solid) 
                ? FillMode.WireFrame
                : FillMode.Solid;
            GraphicsDevice.RasterizerState = r;
        }

        var rotateSpeed = 45f;
        var moveSpeed = 2f;
        
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.LeftControl))
        {
            var rotateDiff = (float)((gameTime.ElapsedGameTime.TotalSeconds / 1f) * rotateSpeed);
            if (_keyboardListener.IsAnyDown(Keys.NumPad8)) _camera.Rotation *= Quaternion.CreateFromAxisAngle(_camera.Up, MathHelper.ToRadians(-rotateDiff));
            if (_keyboardListener.IsAnyDown(Keys.NumPad2)) _camera.Rotation *= Quaternion.CreateFromAxisAngle(_camera.Up, MathHelper.ToRadians(rotateDiff));
            if (_keyboardListener.IsAnyDown(Keys.NumPad4)) _camera.Rotation *= Quaternion.CreateFromAxisAngle(_camera.Right, MathHelper.ToRadians(-rotateDiff));
            if (_keyboardListener.IsAnyDown(Keys.NumPad6)) _camera.Rotation *= Quaternion.CreateFromAxisAngle(_camera.Right, MathHelper.ToRadians(rotateDiff));
        }
        else
        {
            var moveDiff = Vector3.One * (float)((gameTime.ElapsedGameTime.TotalSeconds / 1f) * moveSpeed);
            if (keyboard.IsKeyDown(Keys.W)) _camera.MoveRelative(Vector3.Forward * moveDiff);
            if (keyboard.IsKeyDown(Keys.A)) _camera.MoveRelative(Vector3.Left * moveDiff);
            if (keyboard.IsKeyDown(Keys.S)) _camera.MoveRelative(Vector3.Backward * moveDiff);
            if (keyboard.IsKeyDown(Keys.D)) _camera.MoveRelative(Vector3.Right * moveDiff);
            if (keyboard.IsKeyDown(Keys.Q)) _camera.MoveRelative(Vector3.Up * moveDiff);
            if (keyboard.IsKeyDown(Keys.E)) _camera.MoveRelative(Vector3.Down * moveDiff);
        }

        base.Update(gameTime);
    }
    public ICamera Camera { get; private set; }
    
    protected override void Draw(GameTime gameTime)
    {
        Camera?.Draw(() => base.Draw(gameTime));
    }
}