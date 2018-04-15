using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Alex.API.Gui;
using Alex.API.Input;
using Alex.API.Network;
using Alex.API.World;
using Alex.Gamestates;
using Alex.Gamestates.Gui;
using Alex.Gamestates.Playing;
using Alex.Rendering;
using Alex.ResourcePackLib;
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
		public static IPEndPoint ServerEndPoint { get; set; }
		public static bool IsMultiplayer { get; set; } = false;

		public static SpriteFont Font;
		public static FontRenderer FontRender = null;

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
			if (!File.Exists(Path.Combine("assets", "Minecraftia.xnb")))
			{
				File.WriteAllBytes(Path.Combine("assets", "Minecraftia.xnb"), global::Alex.Resources.Minecraftia);
			}

			Font = Content.Load<SpriteFont>("Minecraftia");

			
			if (!File.Exists(Path.Combine("assets", "SkyboxEffect.xnb")))
			{
				File.WriteAllBytes(Path.Combine("assets", "SkyboxEffect.xnb"), global::Alex.Resources.SkyboxEffect);
			}
			if (!File.Exists(Path.Combine("assets", "CubeModel.xnb")))
			{
				File.WriteAllBytes(Path.Combine("assets", "CubeModel.xnb"), global::Alex.Resources.CubeModel);
			}

			_spriteBatch = new SpriteBatch(GraphicsDevice);
			InputManager = new InputManager(this);
			GuiRenderer = new GuiRenderer(this);
			GuiManager = new GuiManager(this, InputManager, GuiRenderer);
			GameStateManager = new GameStateManager(GraphicsDevice, _spriteBatch, GuiManager);

			var splash = new SplashScreen(this);
			GameStateManager.AddState("splash", splash);
			GameStateManager.SetActiveState("splash");

		//	Log.Info($"Initializing Alex...");
			ThreadPool.QueueUserWorkItem(o => { InitializeGame(splash); });
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

		//	_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap);
		//	FontRender?.DrawString(_spriteBatch, "§1R§2e§3d§4s§5t§6o§7n§8e §9C§ar§be§ca§dt§ei§fo§1n§2s§r§0This is a render test", 10, 10, (int) Color.Black.PackedValue, false, new Vector2(4, 4));
		//	_spriteBatch.End();
			base.Draw(gameTime);
		}

		private void InitializeGame(IProgressReceiver progressReceiver)
		{
			progressReceiver.UpdateProgress(0, "Initializing...");

			Extensions.Init(GraphicsDevice);

			//	Log.Info($"Loading resources...");
			Resources = new ResourceManager(GraphicsDevice);
			if (!Resources.CheckResources(GraphicsDevice, GameSettings, progressReceiver, (f) => { FontRender = (FontRenderer) f; }))
			{
				Exit();
				return;
			}

			Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

			GuiRenderer.LoadResourcePack(Resources.ResourcePack);
			//UiManager.Theme = Resources.UiThemeFactory.GetTheme();

			GameStateManager.AddState("title", new TitleState(this, Content)); 
			GameStateManager.AddState("options", new OptionsState(this));

			if (!BypassTitleState)
			{
				GameStateManager.SetActiveState("title");
			}
			else
			{
				ConnectToServer();
			}

			GameStateManager.RemoveState("splash");
		}

		public void LoadWorld(WorldProvider worldProvider, INetworkProvider networkProvider)
		{
			PlayingState playState = new PlayingState(this, GraphicsDevice, worldProvider, networkProvider);
			GameStateManager.AddState("play", playState);

			LoadingWorldState loadingScreen =
				new LoadingWorldState(this);
			GameStateManager.AddState("loading", loadingScreen);
			GameStateManager.SetActiveState("loading");

			worldProvider.Load(loadingScreen.UpdateProgress).ContinueWith(task =>
			{
				GameStateManager.SetActiveState("play");

				GameStateManager.RemoveState("loading");
			});
		}

		public void ConnectToServer()
		{
			IsMultiplayer = true;

			var javaProvider = new JavaWorldProvider(this, ServerEndPoint, Username, UUID, AccessToken, out INetworkProvider networkProvider);
			LoadWorld(javaProvider, networkProvider);
		}
	}

	public interface IProgressReceiver
	{
		void UpdateProgress(int percentage, string statusMessage);
	}
}