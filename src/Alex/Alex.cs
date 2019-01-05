using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Alex.API;
using Alex.API.Data.Servers;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Input;
using Alex.API.Localization;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.World;
using Alex.Gamestates.Login;
using Alex.GameStates;
using Alex.GameStates.Gui.MainMenu;
using Alex.GameStates.Playing;
using Alex.Gui;
using Alex.Gui.Elements;
using Alex.Networking.Java.Packets;
using Alex.Rendering;
using Alex.ResourcePackLib;
using Alex.Services;
using Alex.Utils;
using Alex.Worlds.Bedrock;
using Alex.Worlds.Java;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using NLog;

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

		public GraphicsDeviceManager DeviceManager { get; }

		public ProfileManager ProfileManager { get; private set; }
		
		internal ConcurrentQueue<Action> UIThreadQueue { get; }

		internal StorageSystem Storage { get; private set; }

		private LaunchSettings LaunchSettings { get; }
		//public ChromiumWebBrowser CefWindow { get; private set; }
		public Alex(LaunchSettings launchSettings)
		{
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
			DebugFont = (WrappedSpriteFont) Content.Load<SpriteFont>(global::Alex.Resources.DebugFont);
			
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			InputManager = new InputManager(this);
			GuiRenderer = new GuiRenderer(this);
			GuiManager = new GuiManager(this, InputManager, GuiRenderer);
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
			Services.AddService<IStorageSystem>(storage);
			Services.AddService<IOptionsProvider>(new OptionsProvider(storage));

			Services.AddService<IListStorageProvider<SavedServerEntry>>(new SavedServerDataProvider(storage));

			Services.AddService<IServerQueryProvider>(new ServerQueryProvider());
			Services.AddService<IPlayerProfileService>(new JavaPlayerProfileService());
			Services.AddService(msa = new XBLMSAService());

			ProfileManager = new ProfileManager(this, storage);
			Storage = storage;

			//msa.AsyncBrowserLogin();
		}

		protected override void UnloadContent()
		{
			SaveSettings();
			ProfileManager.SaveProfiles();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			
			InputManager.Update(gameTime);

			GuiManager.Update(gameTime);
			GameStateManager.Update(gameTime);

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
			}
			else
			{
				GameSettings = new Settings(string.Empty);
				GameSettings.IsDirty = true;
			}

			Extensions.Init(GraphicsDevice);

			ProfileManager.LoadProfiles(progressReceiver);

			//	Log.Info($"Loading resources...");
			Resources = new ResourceManager(GraphicsDevice, Storage);
			if (!Resources.CheckResources(GraphicsDevice, GameSettings, progressReceiver,
				OnResourcePackPreLoadCompleted))
			{
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
	}
}