using System;
using System.Collections.Generic;
using System.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI.Elements;
using RocketUI.Graphics;
using RocketUI.Input;
using RocketUI.Screens;
using RocketUI.Utilities;

namespace RocketUI
{
    public class GuiManager
    {
        private GuiDebugHelper DebugHelper { get; }

        private Game Game { get; }
        private GraphicsDevice GraphicsDevice { get; set; }

        public GuiScaledResolution ScaledResolution { get; }
        public GuiFocusHelper FocusManager { get; }

        public IGuiResourceProvider ResourceProvider { get; }
        public IGuiRenderer GuiRenderer { get; }

        internal InputManager InputManager { get; }
        internal SpriteBatch SpriteBatch { get; private set; }

        public GuiSpriteBatch GuiSpriteBatch { get; private set; }

        public List<GuiScreen> Screens { get; } = new List<GuiScreen>();

        private GuiScreen _debugOverlay;
        private bool _doInit = true;
        
        public GuiManager(Game game, IGuiResourceProvider resourceManager, InputManager inputManager)
        {
            Game = game;
            ResourceProvider = resourceManager;
            InputManager = inputManager;
            ScaledResolution = new GuiScaledResolution(game);
            ScaledResolution.ScaleChanged += ScaledResolutionOnScaleChanged;

            FocusManager = new GuiFocusHelper(this, InputManager, game.GraphicsDevice);

            GuiRenderer = new GuiRenderer(game, resourceManager);
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);

            GuiSpriteBatch = new GuiSpriteBatch(resourceManager, ScaledResolution, Game.GraphicsDevice, SpriteBatch);
            
            DebugHelper = new GuiDebugHelper(this);

            _debugOverlay = new GuiScreen();
            //_debugOverlay.AddChild(new GuiFpsCounter()
            //{
            //    Foreground = Color.White,
            //    Anchor = Anchor.TopLeft,

            //    Font = GuiRenderer.DebugFont
            //});
        }

        private void ScaledResolutionOnScaleChanged(object sender, UiScaleEventArgs args)
        {
            Init(Game.GraphicsDevice);
            
            foreach (var screen in Screens.ToArray())
            {
                screen.UpdateSize(args.ScaledWidth, args.ScaledHeight);
            }

            _debugOverlay.UpdateSize(args.ScaledWidth, args.ScaledHeight);
        }

        public void Init(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            //GuiRenderer.Init(this, graphicsDevice);

            
            GuiSpriteBatch?.Dispose();
            GuiSpriteBatch = new GuiSpriteBatch(ResourceProvider, ScaledResolution, graphicsDevice, SpriteBatch);
        }

        public void Reinitialise()
        {
            _doInit = true;
        }

        public bool HasScreen(GuiScreen screen)
        {
            return Screens.Contains(screen);
        }
        
        public void AddScreen(GuiScreen screen)
        {
            screen.Init(this);
            screen.UpdateSize(ScaledResolution.ScaledWidth, ScaledResolution.ScaledHeight);
            Screens.Add(screen);
        }

        public void RemoveScreen(GuiScreen screen)
        {
            Screens.Remove(screen);
        }

        public void Update(GameTime gameTime)
        {
            ScaledResolution.Update();
            
            var screens = Screens.ToArray();

            if (_doInit)
            {
                _doInit = false;
                
                foreach (var screen in screens)
                {
                    screen.Init(this, true);
                }
            }

            FocusManager.Update(gameTime);

            foreach (var screen in screens)
            {
                screen.Update(gameTime);
            }

            _debugOverlay.Update(gameTime);

            DebugHelper.Update(gameTime);
        }
        
        public void Draw(GameTime gameTime)
        {
            try
            {
                GuiSpriteBatch.Begin();

                ForEachScreen(screen =>
                {
                    screen.Draw(GuiSpriteBatch, gameTime);

                    DebugHelper.DrawScreen(screen);
                });

                _debugOverlay.Draw(GuiSpriteBatch, gameTime);
            }
            finally
            {
                GuiSpriteBatch.End();
            }
        }


        private void ForEachScreen(Action<GuiScreen> action)
        {
            foreach (var screen in Screens.ToArray())
            {
                action.Invoke(screen);
            }
        }
    }
}
