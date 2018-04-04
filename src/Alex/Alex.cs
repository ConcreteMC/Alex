using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Alex.Gamestates;
using Alex.Graphics;
using Alex.Graphics.UI;
using Alex.Rendering;
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

		private SpriteBatch _spriteBatch;

		public static Alex Instance { get; private set; }
		public GameStateManager GameStateManager { get; private set; }
		public ResourceManager Resources { get; private set; }

		public UiManager UiManager { get; private set; }

		public Alex()
		{
			Instance = this;

			var graphics = new GraphicsDeviceManager(this)
			{
				PreferMultiSampling = false,
				SynchronizeWithVerticalRetrace = false,
				GraphicsProfile = GraphicsProfile.Reach
			};
			Content.RootDirectory = "assets";

			IsFixedTimeStep = false;
           // graphics.ToggleFullScreen();

			UiManager = new UiManager(this);

			this.Window.AllowUserResizing = true;
			this.Window.ClientSizeChanged += (sender, args) =>
			{
				if (graphics.PreferredBackBufferWidth != Window.ClientBounds.Width ||
					graphics.PreferredBackBufferHeight != Window.ClientBounds.Height)
				{
					graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
					graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
					graphics.ApplyChanges();
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
				//Log.Info($"Saving settings...");
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
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			UiManager.Init(GraphicsDevice, _spriteBatch);
			GameStateManager = new GameStateManager(GraphicsDevice, _spriteBatch, UiManager);

			GameStateManager.AddState("splash", new SplashScreen(this));
			GameStateManager.SetActiveState("splash");

		//	Log.Info($"Initializing Alex...");
			ThreadPool.QueueUserWorkItem(o => { InitializeGame(); });
		}

		protected override void UnloadContent()
		{
			SaveSettings();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			UiManager.Update(gameTime);
			GameStateManager.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
			GraphicsDevice.Clear(Color.SkyBlue);

			GameStateManager.Draw(gameTime);

			base.Draw(gameTime);
			UiManager.Draw(gameTime);
		}

		private void InitializeGame()
		{
			Extensions.Init(GraphicsDevice);

			if (!File.Exists(Path.Combine("assets", "Minecraftia.xnb")))
			{
				File.WriteAllBytes(Path.Combine("assets", "Minecraftia.xnb"), global::Alex.Resources.Minecraftia1);
			}

			Font = Content.Load<SpriteFont>("Minecraftia");
			//var shader = Content.Load<EffectContent>(Path.Combine("shaders", "hlsl", "renderchunk.vertex"));
			
			//Log.Info($"Loading blockstate metadata...");
			//BlockFactory.Init();

		//	Log.Info($"Loading resources...");
			Resources = new ResourceManager(GraphicsDevice);
			if (!Resources.CheckResources(GraphicsDevice, GameSettings))
			{
				Exit();
				return;
			}

			Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

			UiManager.Theme = Resources.UiThemeFactory.GetTheme();

			//GamestateManager.AddState("login", new LoginState(this));
			//GamestateManager.SetActiveState("login");

			GameStateManager.AddState("title", new TitleState(this)); 
			GameStateManager.AddState("options", new OptionsState(this));

			GameStateManager.SetActiveState("title");

			GameStateManager.RemoveState("splash");

		//	Log.Info($"Game initialized!");
		}
	}
}