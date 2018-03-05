using System;
using System.IO;
using System.Net;
using System.Threading;
using Alex.Gamestates;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace Alex
{
	public partial class Alex : Microsoft.Xna.Framework.Game
	{
		private static ILog Log = LogManager.GetLogger(typeof(Alex));

		public static string Version = "1.0";
		public static string Username { get; set; }
		public static IPEndPoint ServerEndPoint { get; set; }
		public static bool IsMultiplayer { get; set; } = false;

		public static SpriteFont Font;

		private SpriteBatch _spriteBatch;

		public static Alex Instance { get; private set; }
		public GamestateManager GamestateManager { get; private set; }
		public ResourceManager Resources { get; private set; }

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


			IsFixedTimeStep = false;
			//   _graphics.ToggleFullScreen();

			Username = "";
			
		}

		public static EventHandler<TextInputEventArgs> OnCharacterInput;

		private void Window_TextInput(object sender, TextInputEventArgs e)
		{
			OnCharacterInput?.Invoke(this, e);
		}

		public void Init()
		{

		}

		public void SaveSettings()
		{
			if (GameSettings.IsDirty)
			{
				Log.Info($"Saving settings...");
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
					Username = GameSettings.Username;
				}
				catch (Exception ex)
				{
					Log.Warn($"Failed to load settings!", ex);
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
			GamestateManager = new GamestateManager(GraphicsDevice, _spriteBatch);

			GamestateManager.AddState("splash", new SplashScreen(GraphicsDevice));
			GamestateManager.SetActiveState("splash");

			Log.Info($"Initializing Alex...");
			ThreadPool.QueueUserWorkItem(o => { InitializeGame(); });
		}

		protected override void UnloadContent()
		{
			SaveSettings();
		}

		protected override void Update(GameTime gameTime)
		{
			GamestateManager.Update(gameTime);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
			GraphicsDevice.Clear(Color.SkyBlue);

			GamestateManager.Draw(gameTime);

			base.Draw(gameTime);
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
			
			Log.Info($"Loading blockstate metadata...");
			BlockFactory.Init();

			Log.Info($"Loading resources...");
			Resources = new ResourceManager(GraphicsDevice);
			if (!Resources.CheckResources(GraphicsDevice, GameSettings))
			{
				Exit();
				return;
			}

			Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

			GamestateManager.AddState("login", new LoginState(this));
			GamestateManager.SetActiveState("login");

			GamestateManager.RemoveState("splash");

			Log.Info($"Game initialized!");
		}
	}
}