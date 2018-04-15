using System.Collections.Generic;
using Alex.API.Gui.Rendering;
using Alex.API.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui
{
    public class GuiManager
    {
        private Game Game { get; }
        public GuiScaledResolution ScaledResolution { get; }
        public GuiFocusManager FocusManager { get; }

        private InputManager InputManager { get; set; }
        private IGuiRenderer GuiRenderer { get; set; }
        private GraphicsDevice GraphicsDevice { get; set; }
        private SpriteBatch SpriteBatch { get; set; }
        private GuiRenderArgs GuiRenderArgs { get; set; }

        public List<GuiScreen> Screens { get; } = new List<GuiScreen>();
        
        public GuiManager(Game game, InputManager inputManager, IGuiRenderer guiRenderer)
        {
            Game = game;
            InputManager = inputManager;
            ScaledResolution = new GuiScaledResolution(game);
            ScaledResolution.ScaleChanged += ScaledResolutionOnScaleChanged;

            FocusManager = new GuiFocusManager(this, InputManager, game.GraphicsDevice);

            GuiRenderer = guiRenderer;
            SpriteBatch = new SpriteBatch(Game.GraphicsDevice);
            GuiRenderArgs = new GuiRenderArgs(GuiRenderer, Game.GraphicsDevice, SpriteBatch);
        }

        private void ScaledResolutionOnScaleChanged(object sender, UiScaleEventArgs args)
        {
            Init(Game.GraphicsDevice);
            
            foreach (var screen in Screens.ToArray())
            {
                screen.UpdateSize(args.ScaledWidth, args.ScaledHeight);
            }
        }

        public void Init(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            GuiRenderer.Init(graphicsDevice);

            GuiRenderArgs = new GuiRenderArgs(GuiRenderer, GraphicsDevice, SpriteBatch);
        }

        public void AddScreen(GuiScreen screen)
        {
            screen.Init(GuiRenderer);
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
            
            FocusManager.Update(gameTime);

            foreach (var screen in Screens.ToArray())
            {
                screen.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            try
            {
                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None,  RasterizerState.CullNone, null, ScaledResolution.TransformMatrix);

                foreach (var screen in Screens.ToArray())
                {
                    screen.Draw(GuiRenderArgs);
                }
            }
            finally
            {
                SpriteBatch.End();
            }
        }

    }
}
