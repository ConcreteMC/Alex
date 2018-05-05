using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Localization;
using Alex.API.Utils;
using Alex.ResourcePackLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;
using RocketUI.Graphics.Textures;

namespace Alex.Gui
{
    public class AlexGuiResourceProvider : IGuiResourceProvider
    {
            
        public IFont PrimaryFont
        {
            get => _primaryFont;
            set => _primaryFont = value;
        }
        public IFont DebugFont
        {
            get
            {
                if (_debugFont == null && Alex.DebugFont != null)
                {
                    _debugFont = Alex.DebugFont;
                }

                return _debugFont;
            }
            private set { _debugFont = value; }
        }
        
        public CultureLanguage Language = new CultureLanguage(CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.GetCultureInfo("en-US"));
        
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
        
        private Alex Alex { get; }

        private IFont _primaryFont;
        private IFont _debugFont;
        
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<string, GuiTexture2D> _textureCache = new Dictionary<string, GuiTexture2D>();
        private readonly Dictionary<string, IFont> _fontCache = new Dictionary<string, IFont>();

        public AlexGuiResourceProvider(Alex alex, GraphicsDevice graphics)
        {
            Alex = alex;
            _graphicsDevice = graphics;
            LoadEmbeddedTextures();
        }

        public bool TryGetFont(string key, out IFont font)
        {
            return _fontCache.TryGetValue(key, out font);
        }
        public bool TryGetTranslation(string key, out string translation)
        {
            translation = Language[key];
            return true;
        }
        public bool TryGetTexture2D(string key, out ITexture2D texture)
        {
            if (_textureCache.TryGetValue(key, out var guiTexture))
            {
                if (guiTexture.Texture != null)
                {
                    texture = guiTexture;
                    return true;
                }
            }

            texture = default(ITexture2D);
            return false;
        }
        public bool TryGetGuiTexture(string key, out GuiTexture2D texture)
        {
            return _textureCache.TryGetValue(key, out texture);
        }
        public bool TryGetSoundEffect(string key, out SoundEffect soundEffect)
        {
            soundEffect = null;
            return false;
        }
        
        public void LoadResourcePack(McResourcePack resourcePack)
        {
            PrimaryFont = resourcePack.Font;

            LoadLanguages(resourcePack);
            LoadResourcePackTextures(resourcePack);
        }

        #region Private Loading Helpers

        private void LoadLanguages(McResourcePack resourcePack)
        {
            Language.Load(resourcePack.Languages.FirstOrDefault().Value);
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
            resourcePack.TryGetTexture("gui/widgets", out var widgets);
            LoadWidgets(widgets);
            
            resourcePack.TryGetTexture("gui/icons", out var icons);
            LoadIcons(icons);

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
            LoadTextureFromSpriteSheet(GuiTextures.ButtonFocused                       , spriteSheet, WidgetButtonHover);
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

        private void LoadTextureFromEmbeddedResource(string guiTexture, byte[] resource)
        {
            _textureCache[guiTexture] = (TextureSlice2D) TextureUtils.ImageToTexture2D(_graphicsDevice, resource);
        }
        private void LoadTextureFromResourcePack(string textureKey, McResourcePack resourcePack, string path, float scale = 1f)
        {
            if (resourcePack.TryGetTexture(path, out var texture))
            {
                var guiTexture = new GuiTexture2D((TextureSlice2D) texture);
                if (scale != 1f)
                {
                    guiTexture.Scale = new Vector2(scale);
                }

                _textureCache[textureKey] = guiTexture;
            }
        }
        private void LoadTextureFromSpriteSheet(string guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle, Thickness ninePatchThickness)
        {
            _textureCache[guiTexture] = new NinePatchTexture2D(spriteSheet.Slice(sliceRectangle), ninePatchThickness);
        }
        private void LoadTextureFromSpriteSheet(string guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle)
        {
            _textureCache[guiTexture] = spriteSheet.Slice(sliceRectangle);
        }

        #endregion
    }
}
