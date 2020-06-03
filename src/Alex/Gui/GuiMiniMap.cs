using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gamestates;
using Alex.Graphics.Camera;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

namespace Alex.Gui
{
    public class GuiMiniMap : GuiElement
    {
        public Camera Camera { get; }
        public PlayerLocation PlayerLocation { get; set; } = new PlayerLocation();

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
            Anchor = Alignment.TopRight;

            Camera = new TopDownCamera(16);
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);

            InitMiniMap(Alex.Instance.GraphicsDevice);
            Background.Texture = (TextureSlice2D) _mapTexture;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

           // Camera.Position = PlayerLocation;
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            if (_mapTexture == null) return;
        }

        public void Draw(IRenderArgs args)
        {
            if (_mapTexture == null) return;
            Camera.Position = args.Camera.Position;
            
            var device = args.GraphicsDevice;
            var prevRenderTarget = device.GetRenderTargets();
            device.SetRenderTarget(_mapTexture);

            RenderMiniMap(device, args.SpriteBatch, args.GameTime);

            device.SetRenderTargets(prevRenderTarget);
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
            
            device.Clear(Color.Transparent);

            ChunkManager.Draw(renderArgs,false);
        }
    }
}
