using System;
using Alex.Rendering;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Game = Alex.Game;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Alex
{
    public partial class Alex : Microsoft.Xna.Framework.Game
    {
	    public static Alex Instance;

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public Alex()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "assets";

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            Game.Initialize(this);
	        Instance = this;
        }

        public World World { get; private set; }

        protected override void Initialize()
        {
            Console.Title = @"Alex - Debug";
            Window.Title = "Alex - 1.0.0";
            World = new World();
            Game.Init(World.GetSpawnPoint());
            InitCamera();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

			Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
			_originalMouseState = Mouse.GetState();
		}

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Environment.Exit(0);

            UpdateCamera(gameTime);
            if (IsActive)
            {
                HandleInput();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            GraphicsDevice.Clear(Color.SkyBlue);

            World.Render(); //Render the chunks

            FpsCounter.Render(GraphicsDevice); //Render the FPS counter

            base.Draw(gameTime);
        }
    }
}