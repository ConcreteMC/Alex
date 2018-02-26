using System;
using System.IO;
using System.Net;
using Alex.Gamestates;
using Alex.Gamestates.Playing;
using Alex.Properties;
using Alex.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
#if MONOGAME
//using OpenTK;
#endif

namespace Alex
{
	public partial class Alex : Microsoft.Xna.Framework.Game
	{
		public static string Version = "1.0";
		public static string Username { get; set; }
		public static IPEndPoint ServerEndPoint { get; set; }
		public static bool IsMultiplayer { get; set; } = false;

		public static SpriteFont Font;

		private readonly GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		 
		public GamestateManager GamestateManager { get; private set; }
		public ResourceManager Resources { get; private set; }
		public Alex()
		{
              
			_graphics = new GraphicsDeviceManager(this) {
				PreferMultiSampling = false,
				SynchronizeWithVerticalRetrace = false,
				GraphicsProfile = GraphicsProfile.Reach
			};
			Content.RootDirectory = "assets";

			IsFixedTimeStep = false;
         //   _graphics.ToggleFullScreen();
			
			Username = "";
			this.Window.AllowUserResizing = true;
			this.Window.ClientSizeChanged += (sender, args) =>
			{
				if (_graphics.PreferredBackBufferWidth != Window.ClientBounds.Width ||
				    _graphics.PreferredBackBufferHeight != Window.ClientBounds.Height)
				{
					_graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
					_graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
					_graphics.ApplyChanges();
				}
			};
			
		}

		public static EventHandler<char> OnCharacterInput;

#if MONOGAME
		private void Window_TextInput(object sender, TextInputEventArgs e)
		{
			OnCharacterInput?.Invoke(this, e.Character);
		}
#endif
#if FNA
		private void TextInputEXT_TextInput(char obj)
		{
			OnCharacterInput?.Invoke(this, obj);
		}
#endif 

		public void SaveSettings()
		{
			File.WriteAllText("settings.json", JsonConvert.SerializeObject(GameSettings, Formatting.Indented));
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
				catch
				{
					GameSettings = new Settings(string.Empty);
				}
			}
			else
			{
				GameSettings = new Settings(string.Empty);
			}

		   // InitCamera();
#if MONOGAME
			this.Window.TextInput += Window_TextInput;
#endif
#if FNA
			//TextInputEXT.TextInput += TextInputEXT_TextInput;
			//TextInputEXT.StartTextInput();
#endif

			base.Initialize();
		}

		protected override void LoadContent()
		{
			BlockFactory.Init();

			Resources = new ResourceManager(GraphicsDevice);
			Resources.CheckResources(GraphicsDevice, GameSettings);

			_spriteBatch = new SpriteBatch(GraphicsDevice);
			if (!File.Exists(Path.Combine("assets", "Minecraftia.xnb")))
			{
				File.WriteAllBytes(Path.Combine("assets", "Minecraftia.xnb"), Properties.Resources.Minecraftia1);
			}
			Font = Content.Load<SpriteFont>("Minecraftia");

			Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
			//_originalMouseState = Mouse.GetState();

			GamestateManager = new GamestateManager(GraphicsDevice, _spriteBatch);
			GamestateManager.AddState("login", new LoginState(this));
			GamestateManager.SetActiveState("login");

			Extensions.Init(GraphicsDevice);
		}

		protected override void UnloadContent()
		{
#if FNA
		//	TextInputEXT.TextInput -= TextInputEXT_TextInput;
			//TextInputEXT.StopTextInput();
#endif
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
	}
}