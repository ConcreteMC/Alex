using System.Collections.Generic;
using System.Drawing;
using Alex.API;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Alex.ResourcePackLib;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.Gamestates.Gui
{
    public class GuiRenderer : IGuiRenderer
    {

        private Alex Alex { get; }
        
        public BitmapFont Font => Alex.Font;
        public IFontRenderer DefaultFont => Alex.FontRender;

        private GraphicsDevice _graphicsDevice;
        private ResourceManager _resourceManager;

        private Dictionary<GuiTextures, TextureSlice2D> _textureCache = new Dictionary<GuiTextures, TextureSlice2D>();

        private Texture2D _widgets;
        private Texture2D _icons;
        private BitmapFont _font;

        #region SpriteSheet Definitions

        #region Widgets
        
        private static readonly Rectangle WidgetHotBar                = new Rectangle(0, 0, 182, 22);
        private static readonly Rectangle WidgetHotBarSelectedOverlay = new Rectangle(0, 22, 24, 24);
        private static readonly Rectangle WidgetButtonDisabled        = new Rectangle(0, 46, 200, 20);
        private static readonly Rectangle WidgetButtonDefault         = new Rectangle(0, 66, 200, 20);
        private static readonly Rectangle WidgetButtonHover           = new Rectangle(0, 86, 200, 20);

        private static readonly Rectangle WidgetHotBarSeparated = new Rectangle(24, 23, 22, 22);

        #endregion

        #region Icons
        
        private static readonly Rectangle IconCrosshair = new Rectangle(0, 0, 15, 15);

        private static readonly Rectangle IconServerPing5 = new Rectangle(0, 176, 10, 8);
        private static readonly Rectangle IconServerPing4 = new Rectangle(0, 184, 10, 8);
        private static readonly Rectangle IconServerPing3 = new Rectangle(0, 192, 10, 8);
        private static readonly Rectangle IconServerPing2 = new Rectangle(0, 200, 10, 8);
        private static readonly Rectangle IconServerPing1 = new Rectangle(0, 208, 10, 8);
        private static readonly Rectangle IconServerPing0 = new Rectangle(0, 216, 10, 8);

        private static readonly Rectangle IconServerPingPending1 = new Rectangle(10, 176, 10, 8);
        private static readonly Rectangle IconServerPingPending2 = new Rectangle(10, 184, 10, 8);
        private static readonly Rectangle IconServerPingPending3 = new Rectangle(10, 192, 10, 8);
        private static readonly Rectangle IconServerPingPending4 = new Rectangle(10, 200, 10, 8);
        private static readonly Rectangle IconServerPingPending5 = new Rectangle(10, 208, 10, 8);

        #endregion

        #endregion



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

            if (_resourceManager?.ResourcePack != null)
            {
                LoadResourcePack(_resourceManager.ResourcePack);
            }
        }

        public void LoadResourcePack(McResourcePack resourcePack)
        {
            LoadResourcePackTextures(resourcePack);
        }


        private void LoadEmbeddedTextures()
        {
            LoadTextureFromEmbeddedResource(GuiTextures.AlexLogo, Resources.logo2);
            LoadTextureFromEmbeddedResource(GuiTextures.ProgressBar, Resources.ProgressBar);
            LoadTextureFromEmbeddedResource(GuiTextures.SplashBackground, Resources.Splash);
        }


        private void LoadResourcePackTextures(McResourcePack resourcePack)
        {
            // First load Widgets
            resourcePack.TryGetTexture("gui/widgets", out _widgets);
            LoadWidgets(_widgets);
            
            resourcePack.TryGetTexture("gui/icons", out _icons);
            LoadIcons(_icons);

            // Backgrounds
            LoadTextureFromResourcePack(GuiTextures.OptionsBackground, resourcePack, "gui/options_background", 2f);

            // Panorama
            LoadTextureFromResourcePack(GuiTextures.Panorama0, resourcePack, "gui/title/background/panorama_0");
            LoadTextureFromResourcePack(GuiTextures.Panorama1, resourcePack, "gui/title/background/panorama_1");
            LoadTextureFromResourcePack(GuiTextures.Panorama2, resourcePack, "gui/title/background/panorama_2");
            LoadTextureFromResourcePack(GuiTextures.Panorama3, resourcePack, "gui/title/background/panorama_3");
            LoadTextureFromResourcePack(GuiTextures.Panorama4, resourcePack, "gui/title/background/panorama_4");
            LoadTextureFromResourcePack(GuiTextures.Panorama5, resourcePack, "gui/title/background/panorama_5");

            // Other
            LoadTextureFromResourcePack(GuiTextures.DefaultServerIcon, resourcePack, "misc/unknown_server");
        }

        private void LoadWidgets(Texture2D spriteSheet)
        {
            LoadTextureFromSpriteSheet(GuiTextures.Inventory_HotBar                    , spriteSheet, WidgetHotBar);
            LoadTextureFromSpriteSheet(GuiTextures.Inventory_HotBar_SelectedItemOverlay, spriteSheet, WidgetHotBarSelectedOverlay);

            LoadTextureFromSpriteSheet(GuiTextures.ButtonDefault                       , spriteSheet, WidgetButtonDefault);
            LoadTextureFromSpriteSheet(GuiTextures.ButtonHover                         , spriteSheet, WidgetButtonHover);
            LoadTextureFromSpriteSheet(GuiTextures.ButtonDisabled                      , spriteSheet, WidgetButtonDisabled);
            LoadTextureFromSpriteSheet(GuiTextures.PanelGeneric, spriteSheet, WidgetHotBarSeparated, new Thickness(5));
        }

        private void LoadIcons(Texture2D spriteSheet)
        {
            LoadTextureFromSpriteSheet(GuiTextures.Crosshair  , spriteSheet, IconCrosshair);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPing0, spriteSheet, IconServerPing0);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPing1, spriteSheet, IconServerPing1);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPing2, spriteSheet, IconServerPing2);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPing3, spriteSheet, IconServerPing3);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPing4, spriteSheet, IconServerPing4);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPing5, spriteSheet, IconServerPing5);

            LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending1, spriteSheet, IconServerPingPending1);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending2, spriteSheet, IconServerPingPending2);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending3, spriteSheet, IconServerPingPending3);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending4, spriteSheet, IconServerPingPending4);
            LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending5, spriteSheet, IconServerPingPending5);
        }

        private void LoadTextureFromEmbeddedResource(GuiTextures guiTexture, byte[] resource)
        {
            _textureCache[guiTexture] = TextureUtils.ImageToTexture2D(_graphicsDevice, resource);
        }
        
        private void LoadTextureFromResourcePack(GuiTextures guiTexture, McResourcePack resourcePack, string path, float scale = 1f)
        {
            if (resourcePack.TryGetTexture(path, out var texture))
            {
                _textureCache[guiTexture] = texture;
            }
        }
        
        private void LoadTextureFromSpriteSheet(GuiTextures guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle, Thickness ninePatchThickness)
        {
            _textureCache[guiTexture] = new NinePatchTexture2D(spriteSheet.Slice(sliceRectangle), ninePatchThickness);
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
