using System.Collections.Generic;
using Alex.API.Graphics;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.ResourcePackLib;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates.Gui
{
    public class GuiRenderer : IGuiRenderer
    {

        private Alex Alex { get; }

        public SpriteFont DefaultFont => Alex.Font;

        private GraphicsDevice _graphicsDevice;
        private ResourceManager _resourceManager;

        private Dictionary<GuiTextures, TextureSlice2D> _textureCache = new Dictionary<GuiTextures, TextureSlice2D>();

        private Texture2D _widgets;
        private Texture2D _icons;


        private static readonly Rectangle WidgetHotBar = new Rectangle(0, 0, 182, 22);
        private static readonly Rectangle WidgetHotBarSelectedOverlay = new Rectangle(0, 22, 24, 24);
        private static readonly Rectangle WidgetCrosshair = new Rectangle(240, 0, 15, 15);
        private static readonly Rectangle WidgetButtonDisabled = new Rectangle(0, 46, 200, 20);
        private static readonly Rectangle WidgetButtonDefault = new Rectangle(0, 66, 200, 20);
        private static readonly Rectangle WidgetButtonHover = new Rectangle(0, 86, 200, 20);

        private static readonly Rectangle IconCrosshair   = new Rectangle(0, 0  , 10, 8);

        private static readonly Rectangle IconServerPing5 = new Rectangle(0, 176, 10, 8);
        private static readonly Rectangle IconServerPing4 = new Rectangle(0, 184, 10, 8);
        private static readonly Rectangle IconServerPing3 = new Rectangle(0, 192, 10, 8);
        private static readonly Rectangle IconServerPing2 = new Rectangle(0, 200, 10, 8);
        private static readonly Rectangle IconServerPing1 = new Rectangle(0, 208, 10, 8);
        private static readonly Rectangle IconServerPing0 = new Rectangle(0, 216, 10, 8);

        private static readonly Rectangle IconServerConnect1 = new Rectangle(10, 176, 10, 8);
        private static readonly Rectangle IconServerConnect2 = new Rectangle(10, 184, 10, 8);
        private static readonly Rectangle IconServerConnect3 = new Rectangle(10, 192, 10, 8);
        private static readonly Rectangle IconServerConnect4 = new Rectangle(10, 200, 10, 8);
        private static readonly Rectangle IconServerConnect5 = new Rectangle(10, 208, 10, 8);

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
            // First load Sprite Sheets
            resourcePack.TryGetTexture("gui/widgets", out _widgets);
            
            LoadWidgets(_widgets);

            // Backgrounds
            LoadTextureFromResourcePack(GuiTextures.OptionsBackground, resourcePack, "gui/options_background");

            // Panorama
            LoadTextureFromResourcePack(GuiTextures.Panorama0, resourcePack, "gui/title/background/panorama_0");
            LoadTextureFromResourcePack(GuiTextures.Panorama1, resourcePack, "gui/title/background/panorama_1");
            LoadTextureFromResourcePack(GuiTextures.Panorama2, resourcePack, "gui/title/background/panorama_2");
            LoadTextureFromResourcePack(GuiTextures.Panorama3, resourcePack, "gui/title/background/panorama_3");
            LoadTextureFromResourcePack(GuiTextures.Panorama4, resourcePack, "gui/title/background/panorama_4");
            LoadTextureFromResourcePack(GuiTextures.Panorama5, resourcePack, "gui/title/background/panorama_5");
        }

        private void LoadWidgets(Texture2D spriteSheet)
        {
            LoadTextureFromSpriteSheet(GuiTextures.Crosshair                           , spriteSheet, WidgetCrosshair);
            LoadTextureFromSpriteSheet(GuiTextures.Inventory_HotBar                    , spriteSheet, WidgetHotBar);
            LoadTextureFromSpriteSheet(GuiTextures.Inventory_HotBar_SelectedItemOverlay, spriteSheet, WidgetHotBarSelectedOverlay);

            LoadTextureFromSpriteSheet(GuiTextures.ButtonDefault                       , spriteSheet, WidgetButtonDefault);
            LoadTextureFromSpriteSheet(GuiTextures.ButtonHover                         , spriteSheet, WidgetButtonHover);
            LoadTextureFromSpriteSheet(GuiTextures.ButtonDisabled                      , spriteSheet, WidgetButtonDisabled);
        }

        private void LoadTextureFromEmbeddedResource(GuiTextures guiTexture, byte[] resource)
        {
            _textureCache[guiTexture] = TextureUtils.ImageToTexture2D(_graphicsDevice, resource);
        }
        
        private void LoadTextureFromResourcePack(GuiTextures guiTexture, McResourcePack resourcePack, string path)
        {
            if (resourcePack.TryGetTexture(path, out var texture))
            {
                _textureCache[guiTexture] = texture;
            }
        }

        private void LoadTextureFromSpriteSheet(GuiTextures guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle)
        {
            _textureCache[guiTexture] = spriteSheet.Slice(sliceRectangle);
        }

        public TextureSlice2D GetTexture(GuiTextures guiTexture)
        {
            if (_textureCache.TryGetValue(guiTexture, out var texture))
            {
                return texture;
            }

            return (TextureSlice2D)new Texture2D(_graphicsDevice, 1, 1);
        }

        public Texture2D GetTexture2D(GuiTextures guiTexture)
        {
            return GetTexture(guiTexture).Texture;
        }
    }
}
