using System.Collections.Generic;
using System.Drawing;
using Alex.API.Graphics;
using Alex.API.Gui.Rendering;
using Alex.ResourcePackLib;
using Alex.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates.Gui
{
    public class GuiRenderer : IGuiRenderer
    {

        private Alex Alex { get; }

        public SpriteFont DefaultFont => Alex.Font;

        private GraphicsDevice _graphicsDevice;
        private ResourceManager _resourceManager;

        private Dictionary<GuiTextures, NinePatchTexture2D> _textureCache = new Dictionary<GuiTextures, NinePatchTexture2D>();

        private Bitmap _widgets;

        private static readonly Rectangle WidgetHotBar = new Rectangle(0, 0, 182, 22);
        private static readonly Rectangle WidgetHotBarSelectedOverlay = new Rectangle(0, 22, 24, 24);
        private static readonly Rectangle WidgetCrosshair = new Rectangle(240, 0, 15, 15);
        private static readonly Rectangle WidgetButtonDisabled = new Rectangle(0, 46, 200, 20);
        private static readonly Rectangle WidgetButtonDefault = new Rectangle(0, 66, 200, 20);
        private static readonly Rectangle WidgetButtonHover = new Rectangle(0, 86, 200, 20);

        public GuiRenderer(Alex alex)
        {
            Alex = alex;
            Init(alex.GraphicsDevice);
        }
        public void Init(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;
            _resourceManager = Alex.Resources;
            LoadEmbeddedTextures();

            if (_resourceManager != null)
            {
                LoadResourcePackTextures(_resourceManager.ResourcePack);
            }
        }

        private void LoadEmbeddedTextures()
        {
            LoadTextureFromEmbeddedResource(GuiTextures.AlexLogo, Resources.logo2);
            LoadTextureFromEmbeddedResource(GuiTextures.ProgressBar, Resources.ProgressBar);
            LoadTextureFromEmbeddedResource(GuiTextures.SplashBackground, Resources.Splash);
        }


        public void LoadResourcePackTextures(McResourcePack resourcePack)
        {
            // First load Widgets
            resourcePack.TryGetTexture("gui/widgets", out _widgets);

            LoadTextureFromResourcePackWidget(GuiTextures.Crosshair                           , WidgetCrosshair);
            LoadTextureFromResourcePackWidget(GuiTextures.Inventory_HotBar                    , WidgetHotBar);
            LoadTextureFromResourcePackWidget(GuiTextures.Inventory_HotBar_SelectedItemOverlay, WidgetHotBarSelectedOverlay);
            LoadTextureFromResourcePackWidget(GuiTextures.ButtonDefault                       , WidgetButtonDefault);
            LoadTextureFromResourcePackWidget(GuiTextures.ButtonHover                         , WidgetButtonHover);
            LoadTextureFromResourcePackWidget(GuiTextures.ButtonDisabled                      , WidgetButtonDisabled);

            // Load other resources
            LoadTextureFromResourcePack(GuiTextures.TitleScreenBackground, resourcePack, "gui/options_background");
        }

        private void LoadTextureFromEmbeddedResource(GuiTextures texture, byte[] resource)
        {
            _textureCache[texture] = TextureUtils.ImageToTexture2D(_graphicsDevice, resource);
        }
        
        private void LoadTextureFromResourcePack(GuiTextures texture, McResourcePack resourcePack, string path)
        {
            if (resourcePack.TryGetTexture(path, out var bitmap))
            {
                _textureCache[texture] = TextureUtils.ImageToTexture2D(_graphicsDevice, bitmap);
            }
        }

        private void LoadTextureFromResourcePackWidget(GuiTextures texture, Rectangle widgetBounds)
        {
            _textureCache[texture] = _widgets.Slice(_graphicsDevice, widgetBounds);
        }

        public NinePatchTexture2D GetTexture(GuiTextures guiTexture)
        {
            if (_textureCache.TryGetValue(guiTexture, out var texture))
            {
                return texture;
            }

            return (NinePatchTexture2D)new Texture2D(_graphicsDevice, 1, 1);
        }

    }
}
