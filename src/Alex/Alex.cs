using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using Alex.Gui.Forms;
using Alex.Networking.Java.Packets;
using Alex.Services;
using Alex.Utils;
using Alex.Worlds.Bedrock;
using Alex.Worlds.Java;
using Eto.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using GuiDebugHelper = Alex.Gui.GuiDebugHelper;
using TextInputEventArgs = Microsoft.Xna.Framework.TextInputEventArgs;

namespace Alex
{
	public partial class Alex : Microsoft.Xna.Framework.Game
	{
		//private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Alex));

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
		
		private Application EtoApplication { get; }
		public Alex(LaunchSettings launchSettings, Application app)
		{
			EtoApplication = app;
			
			Instance = this;
			LaunchSettings = launchSettings;

			DeviceManager = new GraphicsDeviceManager(this)
			{
				PreferMultiSampling = false,
				SynchronizeWithVerticalRetrace = false,
				GraphicsProfile = GraphicsProfile.HiDef,
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
					DeviceManager.PreferredBackBufferWidth = Window.ClientBounds.Width;
					DeviceManager.PreferredBackBufferHeight = Window.ClientBounds.Height;
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
		}

		public static EventHandler<TextInputEventArgs> OnCharacterInput;

		private void Window_TextInput(object sender, TextInputEventArgs e)
		{
			OnCharacterInput?.Invoke(this, e);
		}

		public void SaveSettings()
		{
			if (GameSettings.IsDirty)
			{
				Storage.TryWrite("settings", GameSettings);
			}
		}

		internal Settings GameSettings { get; private set; }

		protected override void Initialize()
		{
			Window.Title = "Alex - " + Version;

			// InitCamera();
			this.Window.TextInput += Window_TextInput;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			//	if (!File.Exists(Path.Combine("assets", "DebugFont.xnb")))
			//	{
			//		File.WriteAllBytes(Path.Combine("assets", "DebugFont.xnb"), global::Alex.Resources.DebugFont);
			//	}
			//DebugFont = (WrappedSpriteFont) Content.Load<SpriteFont>("DebugFont");
			//CefWindow = new ChromiumWebBrowser(GraphicsDevice, "http://google.com/");

			var fontStream = Assembly.GetEntryAssembly().GetManifestResourceStream("Alex.Resources.DebugFont.xnb");
			
			DebugFont = (WrappedSpriteFont) Content.Load<SpriteFont>(fontStream.ReadAllBytes());
			
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			InputManager = new InputManager(this);
			GuiRenderer = new GuiRenderer(this);
			GuiManager = new GuiManager(this, InputManager, GuiRenderer);
			GuiDebugHelper = new GuiDebugHelper(GuiManager);
			OnCharacterInput += GuiManager.FocusManager.OnTextInput;

			GameStateManager = new GameStateManager(GraphicsDevice, _spriteBatch, GuiManager);

			var splash = new SplashScreen();
			GameStateManager.AddState("splash", splash);
			GameStateManager.SetActiveState("splash");

			//	Log.Info($"Initializing Alex...");
			ThreadPool.QueueUserWorkItem(o => { InitializeGame(splash); });
		}

		private void ConfigureServices()
		{
			XBLMSAService msa;
			var storage = new StorageSystem(LaunchSettings.WorkDir);
			ProfileManager = new ProfileManager(this, storage);
			
			Services.AddService<IStorageSystem>(storage);
			
			var optionsProvider = new OptionsProvider(storage);
			optionsProvider.Load();
			
			Services.AddService<IOptionsProvider>(optionsProvider);

			Services.AddService<IListStorageProvider<SavedServerEntry>>(new SavedServerDataProvider(storage));
			
			Services.AddService(msa = new XBLMSAService(EtoApplication));
			
			Services.AddService<IServerQueryProvider>(new ServerQueryProvider(this));
			Services.AddService<IPlayerProfileService>(new PlayerProfileService(msa, ProfileManager));
			
			Storage = storage;
		}

		protected override void UnloadContent()
		{
			SaveSettings();
			ProfileManager.SaveProfiles();
			
			Services.GetService<IOptionsProvider>().Save();
			GuiDebugHelper.Dispose();
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
				catch { }
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
			GameStateManager.Draw(gameTime);

			GuiManager.Draw(gameTime);
			//	CefWindow.Draw(gameTime);

			base.Draw(gameTime);
		}

		private void InitializeGame(IProgressReceiver progressReceiver)
		{
			MCPacketFactory.Load();
			progressReceiver.UpdateProgress(0, "Initializing...");
			
			ConfigureServices();
			
			if (Storage.TryRead("settings", out Settings settings))
			{
				GameSettings = settings;
				//Console.WriteLine($"OLD SETTINGS: {settings.RenderDistance}");
			}
			else
			{
				GameSettings = new Settings(string.Empty);
				GameSettings.IsDirty = true;
				//Console.WriteLine($"NEW GAMESETTINGS");
			}

			Extensions.Init(GraphicsDevice);

			ProfileManager.LoadProfiles(progressReceiver);

			//	Log.Info($"Loading resources...");
			Resources = new ResourceManager(GraphicsDevice, Storage);
			if (!Resources.CheckResources(GraphicsDevice, GameSettings, progressReceiver,
				OnResourcePackPreLoadCompleted))
			{
                Console.WriteLine("Press enter to exit...");
			    Console.ReadLine();
				Exit();
				return;
			}

			GuiRenderer.LoadResourcePack(Resources.ResourcePack);

			GameStateManager.AddState<TitleState>("title");
			GameStateManager.AddState("options", new OptionsState());

			GameStateManager.SetActiveState<TitleState>("title");


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