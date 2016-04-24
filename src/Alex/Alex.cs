using System;
using System.IO;
using Alex.Gamestates;
using Alex.Properties;
using Alex.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Alex
{
    public partial class Alex : Microsoft.Xna.Framework.Game
    {
	    public static string Version = "1.0";

	    public static Alex Instance;
	    public static SpriteFont Font;

		private Gamestate _gamestate;
		private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public Alex()
        {
            graphics = new GraphicsDeviceManager(this) {PreferMultiSampling = false};

            Content.RootDirectory = "assets";

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Game.Initialize(this);
	        Instance = this;

            ResManager.CheckResources();
            //this.Window.AllowUserResizing = true;
            //this.Window.ClientSizeChanged += (sender, args) =>
            //{
            //    graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            //    graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            //    graphics.ApplyChanges();
            //};

        }

        public World World { get; private set; }

        private object _changeLock = new object();
		public void SetGameState(Gamestate gamestate)
		{
		    lock (_changeLock)
		    {
		        if (_gamestate != null) _gamestate.Stop();

		        _gamestate = gamestate;
		        _gamestate.Init(new RenderArgs {GraphicsDevice = GraphicsDevice});
		    }
		}

		protected override void Initialize()
        {
            Console.Title = @"Alex - Debug";
            Window.Title = "Alex - " + Version;
			SetGameState(new LoginState());

			World = new World();
            Game.Init(World.GetSpawnPoint());
            InitCamera();

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

			base.Draw(gameTime);
        }
    }
}