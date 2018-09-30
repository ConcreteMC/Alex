using System;
using System.Diagnostics;
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
using Alex.Rendering;
using Alex.ResourcePackLib;
using Alex.Services;
using Alex.Utils;
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
		public static string Username { get; set; }
		public static string UUID { get; set; }
		public static string AccessToken { get; set; }
		public static string ClientToken { get; set; }
		public static IPEndPoint ServerEndPoint { get; set; }
		public static bool IsMultiplayer { get; set; } = false;
		public static Texture2D LocalPlayerSkin { get; set; } = null;

		public static IFont Font;
		public static IFont DebugFont;

		private SpriteBatch _spriteBatch;

		public static Alex Instance { get; private set; }
		public GameStateManager GameStateManager { get; private set; }

		public ResourceManager Resources { get; private set; }

		public InputManager InputManager { get; private set; }
		public GuiRenderer GuiRenderer { get; private set; }
		public GuiManager GuiManager { get; private set; }

		private bool BypassTitleState { get; set; } = false;

		public GraphicsDeviceManager DeviceManager { get; }

		public Alex(LaunchSettings launchSettings)
		{
			if (launchSettings.Server != null)
			{
				ServerEndPoint = launchSettings.Server;
				if (launchSettings.ConnectOnLaunch)
				{
					IsMultiplayer = true;
					BypassTitleState = true;
				}
			}

			Username = launchSettings.Username;
			AccessToken = launchSettings.AccesToken;
			UUID = launchSettings.UUID;

			Instance = this;

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
				}
			};
		}

		public static EventHandler<TextInputEventArgs> OnCharacterInput;

		private void Window_TextInput(object sender, TextInputEventArgs e)
		{
			OnCharacterInput?.Invoke(this, e);
		}

		public class JavaInfo
		{
			public string AccessToken;
			public string Username;
			public string RawUsername;
			public string UUID;
			public string ClientToken;
		}

		public static void SaveJava(string rawUsername)
		{
			File.WriteAllText("java.json", JsonConvert.SerializeObject(new JavaInfo()
			{
				AccessToken = AccessToken, Username = Username,
				RawUsername = rawUsername,
				UUID = UUID,
				ClientToken = ClientToken
			}));
		}

		public static bool TryLoadJava(out JavaInfo info)
		{
			info = null;
			if (!File.Exists("java.json")) return false;
			try
			{
				info = JsonConvert.DeserializeObject<JavaInfo>(File.ReadAllText("java.json"));

				Alex.Username = info.Username;
				Alex.AccessToken = info.AccessToken;
				Alex.UUID = info.UUID;
				Alex.ClientToken = info.ClientToken;

				return true;
			}
			catch
			{
				File.Delete("java.json");
				return false;
			}
		}

		public void SaveSettings()
		{
			if (GameSettings.IsDirty)
			{
				File.WriteAllText("settings.json", JsonConvert.SerializeObject(GameSettings, Formatting.Indented));
			}
		}

		internal Settings GameSettings { get; private set; }

		protected override void Initialize()
		{
			Window.Title = "Alex - " + Version;
			
			if (File.Exists("settings.json"))
			{
				try
				{
					GameSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
					if (string.IsNullOrEmpty(Username))
					{
						Username = GameSettings.Username;
					}
				}
				catch (Exception ex)
				{
				//	Log.Warn(ex, $"Failed to load settings!");
				}
			}
			else
			{
				GameSettings = new Settings(string.Empty);
				GameSettings.IsDirty = true;
			}

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
			var storage = new AppDataStorageSystem();
			Services.AddService<IStorageSystem>(storage);
			Services.AddService<IOptionsProvider>(new OptionsProvider(storage));

			Services.AddService<IListStorageProvider<SavedServerEntry>>(new SavedServerDataProvider(storage));

			Services.AddService<IServerQueryProvider>(new ServerQueryProvider());
			Services.AddService<IPlayerProfileService>(new JavaPlayerProfileService());

		}

		protected override void UnloadContent()
		{
			SaveSettings();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			
			InputManager.Update(gameTime);

			GuiManager.Update(gameTime);
			GameStateManager.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
			GameStateManager.Draw(gameTime);

			GuiManager.Draw(gameTime);

			base.Draw(gameTime);
		}

		private void InitializeGame(IProgressReceiver progressReceiver)
		{
			progressReceiver.UpdateProgress(0, "Initializing...");
			ConfigureServices();

			Extensions.Init(GraphicsDevice);

			//	Log.Info($"Loading resources...");
			Resources = new ResourceManager(GraphicsDevice);
			if (!Resources.CheckResources(GraphicsDevice, GameSettings, progressReceiver, OnResourcePackPreLoadCompleted))
			{
				Exit();
				return;
			}

			//Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

			GuiRenderer.LoadResourcePack(Resources.ResourcePack);

			GameStateManager.AddState("selectGameVersion", new VersionSelectionState());
			GameStateManager.AddState<TitleState>("title"); 
			GameStateManager.AddState("options", new OptionsState());

			if (!BypassTitleState)
			{
				GameStateManager.SetActiveState("selectGameVersion");
				//GameStateManager.SetActiveState<TitleState>("title");
			}
			else
			{
				ConnectToServer();
			}

			GameStateManager.RemoveState("splash");
		}

		private void OnResourcePackPreLoadCompleted(IFont font)
		{
			Font = font;

			GuiManager.ApplyFont(font);
		}

		public void ConnectToServer()
		{
			ConnectToServer(ServerEndPoint);
		}

		public void ConnectToServer(IPEndPoint serverEndPoint)
		{
			IsMultiplayer = true;

			var javaProvider = new JavaWorldProvider(this, serverEndPoint, Username, UUID, AccessToken, out INetworkProvider networkProvider);

			LoadWorld(javaProvider, networkProvider);
		}

		public void LoadWorld(WorldProvider worldProvider, INetworkProvider networkProvider)
		{
			PlayingState playState = new PlayingState(this, GraphicsDevice, worldProvider, networkProvider);
			GameStateManager.AddState("play", playState);

			LoadingWorldState loadingScreen = new LoadingWorldState();
			GameStateManager.AddState("loading", loadingScreen);
			GameStateManager.SetActiveState("loading");

			worldProvider.Load(loadingScreen.UpdateProgress).ContinueWith(task =>
			{
				GameStateManager.SetActiveState("play");

				GameStateManager.RemoveState("loading");
			});
		}
	}

	public interface IProgressReceiver
	{
		void UpdateProgress(int percentage, string statusMessage);
	}
}