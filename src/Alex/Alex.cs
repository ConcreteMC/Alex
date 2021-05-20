using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.API;
using Alex.API.Data.Servers;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Input;
using Alex.API.Resources;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Audio;
using Alex.Blocks.Minecraft;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Gamestates.Debugging;
using Alex.Gamestates.InGame;
using Alex.Gamestates.Login;
using Alex.Graphics.Models.Blocks;
using Alex.Gui;
using Alex.Gui.Dialogs.Containers;
using Alex.Gui.Elements;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Play;
using Alex.Particles;
using Alex.Plugins;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Services;
using Alex.Services.Discord;
using Alex.Utils;
using Alex.Utils.Auth;
using Alex.Utils.Tasks;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Multiplayer.Bedrock;
using Alex.Worlds.Multiplayer.Java;
using Alex.Worlds.Singleplayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using NLog;
using RocketUI;
using RocketUI.Debugger;
using RocketUI.Input;
using RocketUI.Input.Listeners;
using RocketUI.Utilities.Extensions;
using RocketUI.Utilities.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using GpuResourceManager = Alex.API.Graphics.GpuResources.GpuResourceManager;
using Image = SixLabors.ImageSharp.Image;
using Point = Microsoft.Xna.Framework.Point;
using Size = RocketUI.Size;
using SpriteBatchExtensions = RocketUI.Utilities.Extensions.SpriteBatchExtensions;
using TextInputEventArgs = Microsoft.Xna.Framework.TextInputEventArgs;

namespace Alex
{
    public class Alex : Microsoft.Xna.Framework.Game
    {
        public static int  MipMapLevel = 6;
        public static bool InGame { get; set; } = false;

        public static EntityModel   PlayerModel   { get; set; }
        public static Image<Rgba32> PlayerTexture { get; set; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Alex));

        public static string Gpu             { get; private set; } = "";
        public static string OperatingSystem { get; private set; } = "";

#if DIRECTX
		public const string RenderingEngine = "DirectX";
#else
        public const string RenderingEngine = "OpenGL";
#endif

        public static string DotnetRuntime { get; } =
            $"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";

        //public const string Version = "1.0 DEV";

        public static bool IsMultiplayer { get; set; } = false;

        public static IFont Font;

        private SpriteBatch _spriteBatch;

        public static Alex             Instance         { get; private set; }
        public        GameStateManager GameStateManager { get; private set; }

        public ResourceManager Resources { get; private set; }

        public InputManager   InputManager   { get; private set; }
        public GuiRenderer    GuiRenderer    { get; private set; }
        public GuiManager     GuiManager     { get; private set; }
        public ParticleManager ParticleManager { get; private set; }

        public GraphicsDeviceManager DeviceManager { get; }

        internal ManagedTaskManager UiTaskManager { get; }

        private LaunchSettings LaunchSettings { get; }
        public  PluginManager  PluginManager  { get; }
        public  FpsMonitor     FpsMonitor     { get; }

        public new IServiceProvider Services { get; set; }

        // public DedicatedThreadPool ThreadPool { get; private set; }

        public StorageSystem     Storage           { get; private set; }
        public ServerTypeManager ServerTypeManager { get; private set; }
        public OptionsProvider   Options           { get; private set; }

        private Point       WindowSize  { get; set; }
        public  AudioEngine AudioEngine { get; set; }
        public Alex(LaunchSettings launchSettings)
        {
            WindowSize = new Point(1280, 750);
            EntityProperty.Factory = new AlexPropertyFactory();

            Instance = this;
            LaunchSettings = launchSettings;

            OperatingSystem =
                $"{System.Runtime.InteropServices.RuntimeInformation.OSDescription} ({System.Runtime.InteropServices.RuntimeInformation.OSArchitecture})";

            DeviceManager = new GraphicsDeviceManager(this)
            {
                PreferMultiSampling = true,
                SynchronizeWithVerticalRetrace = false,
                GraphicsProfile = GraphicsProfile.HiDef,
                PreferHalfPixelOffset = false,
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
            };

            DeviceManager.PreparingDeviceSettings += (sender, args) =>
            {
                Gpu = args.GraphicsDeviceInformation.Adapter.Description;
                args.GraphicsDeviceInformation.PresentationParameters.DepthStencilFormat = DepthFormat.Depth24Stencil8;
                DeviceManager.PreferMultiSampling = true;
                
                DeviceManager.PreferredBackBufferWidth = WindowSize.X;
                DeviceManager.PreferredBackBufferHeight = WindowSize.Y;
            };

            Content = new StreamingContentManager(base.Services, "assets");
            //	Content.RootDirectory = "assets";

            IsFixedTimeStep = false;
            // graphics.ToggleFullScreen();

            this.Window.AllowUserResizing = true;

            this.Window.ClientSizeChanged += (sender, args) =>
            {
                if (DeviceManager.PreferredBackBufferWidth != Window.ClientBounds.Width
                    || DeviceManager.PreferredBackBufferHeight != Window.ClientBounds.Height)
                {
                    if (DeviceManager.IsFullScreen)
                    {
                        DeviceManager.PreferredBackBufferWidth =
                            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;

                        DeviceManager.PreferredBackBufferHeight =
                            GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    }
                    else
                    {
                       DeviceManager.PreferredBackBufferWidth = Window.ClientBounds.Width;
                        DeviceManager.PreferredBackBufferHeight = Window.ClientBounds.Height;
                    }

                    DeviceManager.ApplyChanges();
                }

                var bounds = Window.ClientBounds;
                var viewport = GraphicsDevice.Viewport;
               // viewport.Width = bounds.Width;
               // viewport.Height = bounds.Height;
               // viewport = new Viewport(bounds);
                
               // GraphicsDevice.Viewport = viewport;
            };


            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() {new Texture2DJsonConverter(GraphicsDevice)},
                Formatting = Formatting.Indented
            };

            ServerTypeManager = new ServerTypeManager();
            PluginManager = new PluginManager();

            Storage = new StorageSystem(LaunchSettings.WorkDir);
            Options = new OptionsProvider(Storage);

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<Alex>(this);
            serviceCollection.AddSingleton<Game>(sp => sp.GetRequiredService<Alex>());
            serviceCollection.AddSingleton<ContentManager>(Content);
            serviceCollection.AddSingleton<IStorageSystem>(Storage);
            serviceCollection.AddSingleton<IOptionsProvider>(Options);
            AudioEngine = new AudioEngine(Storage, Options);
            serviceCollection.AddSingleton<Audio.AudioEngine>(AudioEngine);
            
            // RocketUI
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IInputListenerFactory, AlexKeyboardInputListenerFactory>());
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IInputListenerFactory, AlexMouseInputListenerFactory>());
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton<IInputListenerFactory, AlexGamePadInputListenerFactory>());
            serviceCollection.AddSingleton<InputManager>();
            serviceCollection.AddSingleton<GuiRenderer>();
            serviceCollection.AddSingleton<IGuiRenderer, GuiRenderer>(sp => sp.GetRequiredService<GuiRenderer>());
            serviceCollection.AddSingleton<GuiManager>();
            //serviceCollection.AddSingleton<RocketDebugSocketServer>();
           // serviceCollection.AddHostedService<RocketDebugSocketServer>(sp => sp.GetRequiredService<RocketDebugSocketServer>());

            InitiatePluginSystem(serviceCollection);

            serviceCollection.TryAddSingleton<ProfileManager>();

            serviceCollection.TryAddSingleton<IListStorageProvider<SavedServerEntry>, SavedServerDataProvider>();

            serviceCollection.TryAddSingleton<IServerQueryProvider>(new JavaServerQueryProvider(this));
            serviceCollection.TryAddSingleton<IPlayerProfileService, PlayerProfileService>();

            serviceCollection.TryAddSingleton<IRegistryManager, RegistryManager>();

            serviceCollection.TryAddSingleton<ResourceManager>();
            serviceCollection.TryAddSingleton<ServerTypeManager>(ServerTypeManager);
            serviceCollection.TryAddSingleton<XboxAuthService>();

            serviceCollection.TryAddSingleton<BlobCache>();

            Services = serviceCollection.BuildServiceProvider();

            PluginManager.Setup(Services);

            PluginManager.LoadPlugins();

            ServerTypeManager.TryRegister("java", new JavaServerType(this));

            ServerTypeManager.TryRegister(
                "bedrock", new BedrockServerType(this, Services.GetService<XboxAuthService>()));

            FpsMonitor = new FpsMonitor(this);
            FpsMonitor.UpdateOrder = 0;
            Components.Add(FpsMonitor);
            
            UiTaskManager = new ManagedTaskManager(this);
            Components.Add(UiTaskManager);

            Resources = Services.GetRequiredService<ResourceManager>();

            // ThreadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount,
            //    ThreadType.Background, "Dedicated ThreadPool"));

            TextureUtils.RenderThread = Thread.CurrentThread;
            TextureUtils.QueueOnRenderThread = action => UiTaskManager.Enqueue(action);
        }

        /// <inheritdoc />
        protected override void OnExiting(object sender, EventArgs args)
        {
            GpuResourceManager.ReportIncorrectlyDisposedBuffers = false;
            base.OnExiting(sender, args);
        }

        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            if (char.IsLetterOrDigit(e.Character) || char.IsPunctuation(e.Character) || char.IsSymbol(e.Character) || char.IsWhiteSpace(e.Character))
            {
                GuiManager.FocusManager.OnTextInput(this, e);
            }
        }

        private void WindowOnKeyDown(object? sender, InputKeyEventArgs e)
        {
            if (!e.Key.TryConvertKeyboardInput(out _))
            {
                var focusedElement = GuiManager.FocusManager.FocusedElement;

                if (focusedElement != null)
                {
                    focusedElement.InvokeKeyInput('\0', e.Key);
                }
            }
        }

        protected override void Initialize()
        {
            Window.Title = $"Alex - {VersionUtils.GetVersion()} - {RenderingEngine}";

            // InitCamera();
            this.Window.TextInput += Window_TextInput;
            this.Window.KeyDown += WindowOnKeyDown;
            
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var currentAdapter = GraphicsAdapter.Adapters.FirstOrDefault(x => x == GraphicsDevice.Adapter);

                if (currentAdapter != null)
                {
                    if (currentAdapter.IsProfileSupported(GraphicsProfile.HiDef))
                    {
                        DeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
                    }
                }
            }
            base.InactiveSleepTime = TimeSpan.Zero;
           // base.InactiveSleepTime = TimeSpan.FromSeconds(1d / 30d); //Limit framerate to 30fps when window inactive
            
            GraphicsDevice.PresentationParameters.MultiSampleCount = 8;

            DeviceManager.ApplyChanges();

            InputManager = Services.GetRequiredService<InputManager>();
            Components.Add(InputManager);       
            
            base.Initialize();
            
           // RichPresenceProvider.Initialize();
        }

        protected override void LoadContent()
        {
            Stopwatch loadingStopwatch = Stopwatch.StartNew();

            RocketUI.GpuResourceManager.Init(GraphicsDevice);
            
            var builtInFont = ResourceManager.ReadResource("Alex.Resources.default_font.png");

            var image = Image.Load<Rgba32>(builtInFont);
            OnResourcePackPreLoadCompleted(image, McResourcePack.BitmapFontCharacters.ToList());

            var options = Services.GetService<IOptionsProvider>();
            options.Load();

#if DIRECTX
			ResourceManager.BlockEffect = Content.Load<Effect>("Alex.Resources.Blockshader_dx.xnb").Clone();
			ResourceManager.LightingEffect = Content.Load<Effect>("Alex.Resources.Lightmap_dx.xnb").Clone();
#else
            ResourceManager.EntityEffect = Content.Load<Effect>("Alex.Resources.Entityshader.xnb").Clone();
            ResourceManager.BlockEffect = Content.Load<Effect>("Alex.Resources.Blockshader.xnb").Clone();
            ResourceManager.LightingEffect = Content.Load<Effect>("Alex.Resources.Lightmap.xnb").Clone();
#endif
            //	ResourceManager.BlockEffect.GraphicsDevice = GraphicsDevice;

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            InputManager.GetOrAddPlayerManager(PlayerIndex.One); // Init player1's playermanager

            GuiRenderer = Services.GetRequiredService<GuiRenderer>();
            //GuiRenderer.Init(GraphicsDevice);

            GuiManager = Services.GetRequiredService<GuiManager>();
            Components.Add(GuiManager);
            GuiManager.DrawOrder = 100;
            

            options.AlexOptions.VideoOptions.FancyGraphics.Bind(
                (value, newValue) => { Block.FancyGraphics = newValue; });

            Block.FancyGraphics = options.AlexOptions.VideoOptions.FancyGraphics.Value;

            options.AlexOptions.VideoOptions.UseVsync.Bind((value, newValue) => { SetVSync(newValue); });

            if (options.AlexOptions.VideoOptions.UseVsync.Value)
            {
                SetVSync(true);
            }

            options.AlexOptions.VideoOptions.Fullscreen.Bind((value, newValue) => { SetFullscreen(newValue); });

            if (options.AlexOptions.VideoOptions.Fullscreen.Value)
            {
                SetFullscreen(true);
            }

            options.AlexOptions.VideoOptions.LimitFramerate.Bind((value, newValue) =>
                {
                    SetFrameRateLimiter(newValue, options.AlexOptions.VideoOptions.MaxFramerate.Value);
                });

            options.AlexOptions.VideoOptions.MaxFramerate.Bind((value, newValue) =>
                {
                    SetFrameRateLimiter(options.AlexOptions.VideoOptions.LimitFramerate.Value, newValue);
                });

            if (options.AlexOptions.VideoOptions.LimitFramerate.Value)
            {
                SetFrameRateLimiter(true, options.AlexOptions.VideoOptions.MaxFramerate.Value);
            }

            options.AlexOptions.VideoOptions.Antialiasing.Bind((value, newValue) => { SetAntiAliasing(newValue > 0, newValue); });

            options.AlexOptions.MiscelaneousOptions.Language.Bind((value, newValue) => { GuiRenderer.SetLanguage(newValue); });
            
            options.AlexOptions.VideoOptions.SmoothLighting.Bind((value, newValue) => { ResourcePackBlockModel.SmoothLighting = newValue; });

            ResourcePackBlockModel.SmoothLighting = options.AlexOptions.VideoOptions.SmoothLighting.Value;

            SetAntiAliasing(options.AlexOptions.VideoOptions.Antialiasing > 0, options.AlexOptions.VideoOptions.Antialiasing.Value);

            Components.Add(new GuiDebugHelper(this, GuiManager));

            GameStateManager = new GameStateManager(this, GraphicsDevice, _spriteBatch, GuiManager);
            GameStateManager.DrawOrder = 0;
            GameStateManager.UpdateOrder = 0;
            Components.Add(GameStateManager);
            
            var splash = new SplashScreen();
            GameStateManager.AddState("splash", splash);
            GameStateManager.SetActiveState("splash");

            GuiManager.Init();
            
           // if (!GuiRenderer.SetLanguage(options.AlexOptions.MiscelaneousOptions.Language) && !GuiRenderer.SetLanguage(CultureInfo.InstalledUICulture.Name))
            //{
            //    GuiRenderer.SetLanguage("en_uk");
           // }

            GuiManager.ScaledResolution.TargetWidth = 320;
            GuiManager.ScaledResolution.TargetHeight = 240;
            GuiManager.ScaledResolution.GuiScale = Options.AlexOptions.VideoOptions.GuiScale.Value;
            Options.AlexOptions.VideoOptions.GuiScale.Bind(GuiScaleChanged);
            
            ParticleManager = new ParticleManager(this, GraphicsDevice, Resources);
            ParticleManager.Enabled = Options.AlexOptions.VideoOptions.Particles.Value;

            Options.AlexOptions.VideoOptions.Particles.Bind(
                (old, newValue) =>
                {
                    ParticleManager.Enabled = newValue;
                });
            
            Components.Add(ParticleManager);
            
            //	Log.Info($"Initializing Alex...");
            ThreadPool.QueueUserWorkItem(
                async (o) =>
                {
                    try
                    {
                        await Task.WhenAll(Services.GetServices<IHostedService>().Select(s => s.StartAsync(CancellationToken.None)));
                        
                        InitializeGame(splash);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Could not initialize! {ex}");
                    }
                    finally
                    {
                        loadingStopwatch.Stop();
                        Log.Info($"Startup time: {loadingStopwatch.Elapsed}");
                    }
                });
        }

        private void GuiScaleChanged(int oldvalue, int newvalue)
        {
            GuiManager.ScaledResolution.GuiScale = newvalue;
        }

        private void InitiatePluginSystem(IServiceCollection serviceCollection)
        {
            string pluginDirectoryPaths =
                Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            var pluginDir = Options.AlexOptions.ResourceOptions.PluginDirectory;

            if (!string.IsNullOrWhiteSpace(pluginDir))
            {
                pluginDirectoryPaths = pluginDir;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(LaunchSettings.WorkDir) && Directory.Exists(LaunchSettings.WorkDir))
                {
                    pluginDirectoryPaths = LaunchSettings.WorkDir;
                }
            }

            List<string> paths = new List<string>();

            foreach (string dirPath in pluginDirectoryPaths.Split(
                new char[] {';'}, StringSplitOptions.RemoveEmptyEntries))
            {
                string directory = dirPath;

                if (!Path.IsPathRooted(directory))
                {
                    directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dirPath);
                }

                paths.Add(directory);
                //PluginManager.DiscoverPlugins(directory);
            }

            PluginManager.DiscoverPlugins(paths.ToArray());
            PluginManager.ConfigureServices(serviceCollection);
        }

        private void SetAntiAliasing(bool enabled, int count)
        {
            UiTaskManager.Enqueue(
                () =>
                {
                    DeviceManager.PreferMultiSampling = enabled;
                    GraphicsDevice.PresentationParameters.MultiSampleCount = count;

                    DeviceManager.ApplyChanges();
                });
        }

        private void SetFrameRateLimiter(bool enabled, int frameRateLimit)
        {
            UiTaskManager.Enqueue(
                () =>
                {
                    base.IsFixedTimeStep = enabled;
                    base.TargetElapsedTime = TimeSpan.FromSeconds(1d / frameRateLimit);
                });
        }

        private void SetVSync(bool enabled)
        {
            UiTaskManager.Enqueue(
                () =>
                {
                    base.IsFixedTimeStep = enabled;
                    DeviceManager.SynchronizeWithVerticalRetrace = enabled;
                    DeviceManager.ApplyChanges();
                });
        }

        private void SetFullscreen(bool enabled)
        {
            UiTaskManager.Enqueue(
                () =>
                {
                    if (this.DeviceManager.IsFullScreen != enabled)
                    {
                        if (enabled)
                        {
                            WindowSize = Window.ClientBounds.Size;
                        }
                        else
                        {
                            DeviceManager.PreferredBackBufferWidth = WindowSize.X;
                            DeviceManager.PreferredBackBufferHeight = WindowSize.Y;
                            this.DeviceManager.ApplyChanges();
                        }

                        this.DeviceManager.IsFullScreen = enabled;
                        this.DeviceManager.ApplyChanges();
                    }
                });
        }
        
        protected override void UnloadContent()
        {
            base.UnloadContent();
            //ProfileManager.SaveProfiles();
            Options.Save();

           // GuiDebugHelper.Dispose();

            PluginManager.UnloadAll();
            
            RichPresenceProvider.ClearPresence();
        }

        /// <inheritdoc />
        protected override void EndDraw()
        {
            Metrics = GraphicsDevice.Metrics;
            base.EndDraw();
        }
        
        public GraphicsMetrics Metrics { get; private set; }

        private void InitializeGame(IProgressReceiver progressReceiver)
        {
            progressReceiver.UpdateProgress(0, "Initializing...");

            SpriteBatchExtensions.Init(GraphicsDevice);
            API.Extensions.Init(GraphicsDevice);
            MCPacketFactory.Load();
            //ConfigureServices();

            //var options = Services.GetService<IOptionsProvider>();

            //	Log.Info($"Loading resources...");
            if (!Resources.CheckResources(GraphicsDevice, progressReceiver, OnResourcePackPreLoadCompleted))
            {
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
                Exit();

                return;
            }

            var profileManager = Services.GetService<ProfileManager>();
            profileManager.LoadProfiles(progressReceiver);

            //GuiRenderer.LoadResourcePack(Resources.ResourcePack, null);
            AnvilWorldProvider.LoadBlockConverter();

            PluginManager.EnablePlugins();

            var storage = Services.GetRequiredService<IStorageSystem>();

            if (storage.TryReadString("skin.json", out var str, Encoding.UTF8))
            {
                Dictionary<string, EntityModel> models = new Dictionary<string, EntityModel>();
                BedrockResourcePack.LoadEntityModel(str, models);
                models = BedrockResourcePack.ProcessEntityModels(models);

                if (models.Count > 0)
                {
                    if (models.TryGetValue("geometry.humanoid.custom", out var entityModel) || models.TryGetValue(
                        "geometry.humanoid.customSlim", out entityModel))
                    {
                        PlayerModel = entityModel;
                        Log.Info($"Player model loaded...");
                    }
                }
            }

            if (PlayerModel == null)
            {
                if (ModelFactory.TryGetModel("geometry.humanoid.customSlim", out var model))
                {
                    //model.Name = "geometry.humanoid.customSlim";
                    PlayerModel = model;
                }
            }

            if (PlayerModel != null)
            {
                //Log.Info($"Player model loaded...");
            }

            if (storage.TryReadBytes("skin.png", out byte[] skinBytes))
            {
                using (var skinImage = Image.Load<Rgba32>(skinBytes))
                {
                    var modelTextureSize = new Point(0, 0);

                    if (PlayerModel.Description != null)
                    {
                        modelTextureSize.X = (int) PlayerModel.Description.TextureWidth;
                        modelTextureSize.Y = (int) PlayerModel.Description.TextureHeight;
                    }

                    if (modelTextureSize.Y > skinImage.Height)
                    {
                        PlayerTexture = SkinUtils.ConvertSkin(skinImage, modelTextureSize.X, modelTextureSize.Y);
                    }
                    else
                    {
                        PlayerTexture = skinImage.Clone(); //.Clone<Rgba32>();
                    }
                }
            }
            else
            {
                if (Resources.TryGetBitmap("entity/alex", out var img))
                {
                    PlayerTexture = img;
                }
            }

            if (PlayerTexture != null)
            {
                Log.Info($"Player skin loaded...");
            }

            if (LaunchSettings.ModelDebugging)
            {
                GameStateManager.SetActiveState<ModelDebugState>();
            }
            else
            {
                GameStateManager.SetActiveState<TitleState>("title");
            }

            GameStateManager.RemoveState("splash");
        }

        private void OnResourcePackPreLoadCompleted(Image<Rgba32> fontBitmap, List<char> bitmapCharacters)
        {
            UiTaskManager.Enqueue(
                () =>
                {
                    Font = new BitmapFont(GraphicsDevice, fontBitmap, 16, bitmapCharacters);

                    GuiManager.ApplyFont(Font);
                });
        }

        public void ConnectToServer(ServerTypeImplementation serverType,
            ServerConnectionDetails                          connectionDetails,
            PlayerProfile                                    profile)
        {
            try
            {
                WorldProvider   provider;
                NetworkProvider networkProvider;
                IsMultiplayer = true;

                if (serverType.TryGetWorldProvider(connectionDetails, profile, out provider, out networkProvider))
                {
                    LoadWorld(provider, networkProvider, true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"FCL: {ex.ToString()}");
            }
        }

        public void LoadWorld(WorldProvider worldProvider, NetworkProvider networkProvider, bool isServer = false)
        {
            var state       = new PlayingState(this, GraphicsDevice, worldProvider, networkProvider);
            var parentState = GameStateManager.GetActiveState();

            if (parentState is PlayingState)
                parentState = null;

            LoadingWorldScreen loadingScreen = new LoadingWorldScreen();
            loadingScreen.ConnectingToServer = isServer;
            loadingScreen.CancelAction = () =>
            {
                GuiManager.RemoveScreen(loadingScreen);
                //playState?.Unload();
                worldProvider?.Dispose();
                
                GameStateManager.RemoveState("play");
                state?.Unload();
            };

            GuiManager.AddScreen(loadingScreen);
            //GameStateManager.AddState("loading", loadingScreen);
            //GameStateManager.SetActiveState("loading");

            ThreadPool.QueueUserWorkItem(
                o =>
                {
                    LoadResult result = LoadResult.Timeout;
                    try
                    {
                        GameStateManager.RemoveState("play");

                        result = worldProvider.Load(loadingScreen.UpdateProgress);
                        GameStateManager.AddState("play", state);
                        
                        if (networkProvider.IsConnected && result == LoadResult.Done)
                        {
                            GameStateManager.SetActiveState("play");

                            return;
                        }
                    }
                    finally
                    {
                        GuiManager.RemoveScreen(loadingScreen);
                        //GameStateManager.RemoveState("loading");

                        if (result != LoadResult.Done)
                        {
                            if (result != LoadResult.Aborted &&
                                !(GameStateManager.GetActiveState() is DisconnectedScreen))
                            {
                                var s = new DisconnectedScreen();
                                s.DisconnectedTextElement.TranslationKey = "multiplayer.status.cannot_connect";
                                s.ParentState = parentState;
                                GameStateManager.SetActiveState(s, false);

                                //playState?.Unload();
                                //worldProvider?.Dispose();
                                //state?.Unload();
                            }

                            worldProvider?.Dispose();
                            GameStateManager.RemoveState("play");
                            // state?.Unload();
                        }
                    }
                });
        }
    }

    public interface IProgressReceiver
    {
        void UpdateProgress(int percentage, string statusMessage);
        void UpdateProgress(int percentage, string statusMessage, string sub);

        void UpdateProgress(int done, int total, string statusMessage) =>
            UpdateProgress((int) (((double) done / (double) total) * 100D), statusMessage);

        void UpdateProgress(int done, int total, string statusMessage, string sub) =>
            UpdateProgress((int) (((double) done / (double) total) * 100D), statusMessage, sub);
    }
   
}