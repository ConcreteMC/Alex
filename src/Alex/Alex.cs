using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Alex.API.Data.Servers;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Input;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.World;
using Alex.GameStates;
using Alex.GameStates.Gui.MainMenu;
using Alex.GameStates.Playing;
using Alex.Gui;
using Alex.Networking.Java.Packets;
using Alex.Plugins;
using Alex.Services;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Bedrock;
using Alex.Worlds.Java;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using Newtonsoft.Json;
using NLog;
using StackExchange.Profiling;
using GuiDebugHelper = Alex.Gui.GuiDebugHelper;
using TextInputEventArgs = Microsoft.Xna.Framework.TextInputEventArgs;

namespace Alex
{
	public partial class Alex : Microsoft.Xna.Framework.Game
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Alex));

		public static string DotnetRuntime { get; } =
			$"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";

		public const string Version = "1.0 DEV";

		public static bool IsMultiplayer { get; set; } = false;

		public static IFont Font;
		public static IFont DebugFont;

		private SpriteBatch _spriteBatch;

		public static Alex Instance { get; private set; }
		public GameStateManager GameStateManager { get; private set; }

		public ResourceManager Resources { get; private set; }

		public InputManager InputManager { get; private set; }
		public GuiRenderer GuiRenderer { get; private set; }
		public GuiManager GuiManager { get; private set; }
		public GuiDebugHelper GuiDebugHelper { get; private set; }

		public GraphicsDeviceManager DeviceManager { get; }

		public ProfileManager ProfileManager { get; private set; }
		
		internal ConcurrentQueue<Action> UIThreadQueue { get; }

		internal StorageSystem Storage { get; private set; }

		private LaunchSettings LaunchSettings { get; }
		//public ChromiumWebBrowser CefWindow { get; private set; }
		public PluginManager PluginManager { get; }
        public FpsMonitor FpsMonitor { get; }
        private IPlayerProfileService ProfileService { get; set; }
        public Alex(LaunchSettings launchSettings)
		{
			Instance = this;
			LaunchSettings = launchSettings;

			DeviceManager = new GraphicsDeviceManager(this)
			{
				PreferMultiSampling = false,
				SynchronizeWithVerticalRetrace = false,
				GraphicsProfile = GraphicsProfile.Reach,
			};
			

			Content.RootDirectory = "assets";

			IsFixedTimeStep = false;
           // graphics.ToggleFullScreen();
			
			this.Window.AllowUserResizing = true;
			this.Window.ClientSizeChanged += (sender, args) =>
			{
				if (DeviceManager.PreferredBackBufferWidth != Window.ClientBounds.Width ||
				    DeviceManager.PreferredBackBufferHeight != Window.ClientBounds.Height)
				{
					if (DeviceManager.IsFullScreen)
					{
						DeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
						DeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
					}
					else
					{
						DeviceManager.PreferredBackBufferWidth = Window.ClientBounds.Width;
						DeviceManager.PreferredBackBufferHeight = Window.ClientBounds.Height;
					}

					DeviceManager.ApplyChanges();

					//CefWindow.Size = new System.Drawing.Size(Window.ClientBounds.Width, Window.ClientBounds.Height);
				}
			};
			

			JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
			{
				Converters = new List<JsonConverter>()
				{
					new Texture2DJsonConverter(GraphicsDevice)
				},
				Formatting = Formatting.Indented
			};

			UIThreadQueue = new ConcurrentQueue<Action>();

            PluginManager = new PluginManager(this);
            FpsMonitor = new FpsMonitor();
		}

		public static EventHandler<TextInputEventArgs> OnCharacterInput;

		private void Window_TextInput(object sender, TextInputEventArgs e)
		{
			OnCharacterInput?.Invoke(this, e);
		}

		protected override void Initialize()
		{
			Window.Title = "Alex - " + Version;

			// InitCamera();
			this.Window.TextInput += Window_TextInput;
			
			var currentAdapter = GraphicsAdapter.Adapters.FirstOrDefault(x => x == GraphicsDevice.Adapter);
			if (currentAdapter != null)
			{
				if (currentAdapter.IsProfileSupported(GraphicsProfile.HiDef))
				{
					DeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
				}
			}
			
			DeviceManager.ApplyChanges();
			
			base.Initialize();
		}

		protected override void LoadContent()
		{
			var fontStream = Assembly.GetEntryAssembly().GetManifestResourceStream("Alex.Resources.DebugFont.xnb");
			
			DebugFont = (WrappedSpriteFont) Content.Load<SpriteFont>(fontStream.ReadAllBytes());
			
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			InputManager = new InputManager(this);
			GuiRenderer = new GuiRenderer(this);
			GuiManager = new GuiManager(this, InputManager, GuiRenderer);

			GuiDebugHelper = new GuiDebugHelper(GuiManager);

			AlexIpcService = new AlexIpcService();
			Services.AddService<AlexIpcService>(AlexIpcService);
			AlexIpcService.Start();

			OnCharacterInput += GuiManager.FocusManager.OnTextInput;

			GameStateManager = new GameStateManager(GraphicsDevice, _spriteBatch, GuiManager);

			var splash = new SplashScreen();
			GameStateManager.AddState("splash", splash);
			GameStateManager.SetActiveState("splash");

			WindowSize = this.Window.ClientBounds.Size;
			//	Log.Info($"Initializing Alex...");
			ThreadPool.QueueUserWorkItem((o) => { InitializeGame(splash); });
		}

		private AlexIpcService AlexIpcService;

		private void SetVSync(bool enabled)
		{
			UIThreadQueue.Enqueue(() =>
			{
				base.IsFixedTimeStep = enabled;
				DeviceManager.SynchronizeWithVerticalRetrace = enabled;
				DeviceManager.ApplyChanges();
			});
		}

		private Point WindowSize { get; set; }
		private void SetFullscreen(bool enabled)
		{
			UIThreadQueue.Enqueue(() =>
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
						DeviceManager.PreferredBackBufferHeight =WindowSize.Y;
						this.DeviceManager.ApplyChanges();
					}
					
					this.DeviceManager.IsFullScreen = enabled;
					this.DeviceManager.ApplyChanges();
				}
			});
		}
		
		private void ConfigureServices()
		{
			XBLMSAService msa;
			var storage = new StorageSystem(LaunchSettings.WorkDir);
			ProfileManager = new ProfileManager(this, storage);


			Services.AddService<IStorageSystem>(storage);
			
			var optionsProvider = new OptionsProvider(storage);
			optionsProvider.Load();
			
			optionsProvider.AlexOptions.VideoOptions.UseVsync.Bind((value, newValue) => { SetVSync(newValue); });
			if (optionsProvider.AlexOptions.VideoOptions.UseVsync.Value)
			{
				SetVSync(true);
			}
			
			optionsProvider.AlexOptions.VideoOptions.Fullscreen.Bind((value, newValue) => { SetFullscreen(newValue); });
			if (optionsProvider.AlexOptions.VideoOptions.Fullscreen.Value)
			{
				SetFullscreen(true);
			}
			
			Services.AddService<IOptionsProvider>(optionsProvider);

			Services.AddService<IListStorageProvider<SavedServerEntry>>(new SavedServerDataProvider(storage));
			
			Services.AddService(msa = new XBLMSAService());
			
			Services.AddService<IServerQueryProvider>(new ServerQueryProvider(this));
			Services.AddService<IPlayerProfileService>(ProfileService = new PlayerProfileService(msa, ProfileManager));

            var profilingService = new ProfilerService();
            Services.AddService<ProfilerService>(profilingService);

            Storage = storage;
		}

		protected override void UnloadContent()
		{
			ProfileManager.SaveProfiles();
			
			Services.GetService<IOptionsProvider>().Save();
			AlexIpcService.Stop();
			GuiDebugHelper.Dispose();

            PluginManager.UnloadAll();
        }

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			
			InputManager.Update(gameTime);

			GuiManager.Update(gameTime);
			GameStateManager.Update(gameTime);
			GuiDebugHelper.Update(gameTime);

			if (UIThreadQueue.TryDequeue(out Action a))
			{
				try
				{
					a.Invoke();
				}
				catch (Exception ex)
				{
					Log.Warn($"Exception on UIThreadQueue: {ex.ToString()}");
				}
			}
		}

		protected override void Draw(GameTime gameTime)
		{
            FpsMonitor.Update();
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            
            GameStateManager.Draw(gameTime);
			GuiManager.Draw(gameTime);
			
			base.Draw(gameTime);
		}

		private void InitializeGame(IProgressReceiver progressReceiver)
		{
			progressReceiver.UpdateProgress(0, "Initializing...");
			Extensions.Init(GraphicsDevice);
			MCPacketFactory.Load();

			ConfigureServices();

			var options = Services.GetService<IOptionsProvider>();

			string pluginDirectoryPaths = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            var pluginDir = options.AlexOptions.ResourceOptions.PluginDirectory;
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

            foreach (string dirPath in pluginDirectoryPaths.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string directory = dirPath;
                if (!Path.IsPathRooted(directory))
                {
                    directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dirPath);
                }

                PluginManager.DiscoverPlugins(directory);
            }


            ProfileManager.LoadProfiles(progressReceiver);

			//	Log.Info($"Loading resources...");
			Resources = new ResourceManager(GraphicsDevice, Storage, options);
			if (!Resources.CheckResources(GraphicsDevice, progressReceiver,
				OnResourcePackPreLoadCompleted))
			{
                Console.WriteLine("Press enter to exit...");
			    Console.ReadLine();
				Exit();
				return;
			}

			GuiRenderer.LoadResourcePack(Resources.ResourcePack);
			AnvilWorldProvider.LoadBlockConverter();

			GameStateManager.AddState<TitleState>("title");

			if (LaunchSettings.ConnectOnLaunch && ProfileService.CurrentProfile != null)
			{
				ConnectToServer(LaunchSettings.Server, ProfileService.CurrentProfile, LaunchSettings.ConnectToBedrock);
			}
			else
			{
				GameStateManager.SetActiveState<TitleState>("title");
			}

			GameStateManager.RemoveState("splash");

		}

		private void OnResourcePackPreLoadCompleted(IFont font)
		{
			Font = font;

			GuiManager.ApplyFont(font);
		}

		public void ConnectToServer(IPEndPoint serverEndPoint, PlayerProfile profile, bool bedrock = false)
		{
			WorldProvider provider;
			INetworkProvider networkProvider;
			IsMultiplayer = true;
			if (bedrock)
			{
				provider = new BedrockWorldProvider(this, serverEndPoint,
					profile, out networkProvider);
			}
			else
			{
				provider = new JavaWorldProvider(this, serverEndPoint, profile,
					out networkProvider);
			}

			LoadWorld(provider, networkProvider);
		}

		public void LoadWorld(WorldProvider worldProvider, INetworkProvider networkProvider)
		{
			GameStateManager.RemoveState("play");

			PlayingState playState = new PlayingState(this, GraphicsDevice, worldProvider, networkProvider);
			GameStateManager.AddState("play", playState);

			LoadingWorldState loadingScreen = new LoadingWorldState();
			GameStateManager.AddState("loading", loadingScreen);
			GameStateManager.SetActiveState("loading");

			worldProvider.Load(loadingScreen.UpdateProgress).ContinueWith(task =>
			{
				if (networkProvider.IsConnected)
				{
					GameStateManager.SetActiveState("play");
				}
				else
				{
					GameStateManager.RemoveState("play");
					worldProvider.Dispose();
				}

				GameStateManager.RemoveState("loading");
			});
		}
	}

    public interface IProgressReceiver
	{
		void UpdateProgress(int percentage, string statusMessage);
		void UpdateProgress(int percentage, string statusMessage, string sub);
    }
}