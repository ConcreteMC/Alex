using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.Rendering;
using Alex.Rendering.Camera;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Graphics;
using RocketUI.Graphics.Textures;

namespace Alex.Gui
{
    public class GuiMiniMap : VisualElement
    {
        public Camera Camera { get; }
        public PlayerLocation PlayerLocation { get; set; }

        private ChunkManager ChunkManager { get; }

        private RenderTarget2D _mapTexture;

        public GuiMiniMap(ChunkManager chunkManager)
        {
            ChunkManager = chunkManager;

            Background = Color.WhiteSmoke;
            
            AutoSizeMode = AutoSizeMode.None;
            Width = 128;
            Height = 128;
            Margin = new Thickness(10, 10);
            Anchor = Anchor.TopRight;

            Camera = new TopDownCamera(16);
        }

        protected override void OnInit()
        {
            base.OnInit();

            InitMiniMap(Alex.Instance.GraphicsDevice);
            Background = (TextureSlice2D) _mapTexture;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            Camera.Position = PlayerLocation;
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            if (_mapTexture == null) return;

            var device = graphics.Context.GraphicsDevice;
            device.SetRenderTarget(_mapTexture);

            RenderMiniMap(device, graphics.SpriteBatch, gameTime);

            device.SetRenderTarget(null);
        }

        
        private void InitMiniMap(GraphicsDevice device)
        {
            _mapTexture = new RenderTarget2D(device, Width, Height, false, SurfaceFormat.Color, DepthFormat.None);

        }

        private void RenderMiniMap(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
        {
            var bounds = _mapTexture.Bounds;
            var center = bounds.Center;

            var renderArgs = new RenderArgs()
            {
                Camera         = Camera,
                GameTime       = gameTime,
                SpriteBatch    = spriteBatch,
                GraphicsDevice = device
            };

            ChunkManager.Draw(renderArgs);
        }
    }
}
