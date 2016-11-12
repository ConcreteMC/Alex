using System;
using System.IO;
using System.Net;
using Alex.Gamestates;
using Alex.Properties;
using Alex.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using OpenTK;

namespace Alex
{
    public partial class Alex : Microsoft.Xna.Framework.Game
    {
	    public static string Version = "1.0";
        public static string Username { get; set; }
        public static IPEndPoint ServerEndPoint { get; set; }
        public static bool IsMultiplayer { get; set; } = false;

	    public static Alex Instance;
	    public static SpriteFont Font;

		private Gamestate _gamestate;
		private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public Alex()
        {
            graphics = new GraphicsDeviceManager(this) {
                PreferMultiSampling = false,
                SynchronizeWithVerticalRetrace = false
            };

            Content.RootDirectory = "assets";

            IsFixedTimeStep = false;
            Game.Initialize(this);
	        Instance = this;

            ResManager.CheckResources();
            Username = "";
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += (sender, args) =>
            {
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                graphics.ApplyChanges();
            };
            
        }

        public EventHandler<char> OnCharacterInput;

        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            OnCharacterInput?.Invoke(this, e.Character);
        }

        public void SaveSettings()
        {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(GameSettings, Formatting.Indented));
        }

        public World World { get; private set; }

        private readonly object _changeLock = new object();
		public void SetGameState(Gamestate gamestate, bool stopOld = true, bool init = true)
		{
		    lock (_changeLock)
		    {
		        if (stopOld && _gamestate != null)
		        {
		            _gamestate.Stop();
		        }

		        _gamestate = gamestate;

		        if (init)
		        {
		            _gamestate.Init(new RenderArgs {GraphicsDevice = GraphicsDevice});
		        }
		    }
		}

        internal Settings GameSettings { get; private set; }
		protected override void Initialize()
        {
            Console.Title = @"Alex - Debug";
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

            SetGameState(new LoginState());

			World = new World();
            Game.Init(World.GetSpawnPoint());
            InitCamera();

            this.Window.TextInput += Window_TextInput;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            if (!File.Exists(Path.Combine("assets", "Minecraftia.xnb")))
            {
                File.WriteAllBytes(Path.Combine("assets", "Minecraftia.xnb"), Resources.Minecraftia1);
            }
			Font = Content.Load<SpriteFont>("Minecraftia");

			Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
			_originalMouseState = Mouse.GetState();
		}

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            lock (_changeLock)
            {
                _gamestate.UpdateCall(gameTime);

                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            GraphicsDevice.Clear(Color.SkyBlue);

			lock (_changeLock)
			{
			    _gamestate.Rendering3D(new RenderArgs
			    {
			        GraphicsDevice = GraphicsDevice,
			        GameTime = gameTime,
			        SpriteBatch = spriteBatch
			    });

			    _gamestate.Rendering2D(new RenderArgs
			    {
			        GraphicsDevice = GraphicsDevice,
			        GameTime = gameTime,
			        SpriteBatch = spriteBatch
			    });
			}

			base.Draw(gameTime);
        }
    }
}