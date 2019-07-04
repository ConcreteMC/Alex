using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Graphics;
using Alex.API.Localization;
using Alex.API.Utils;
using Alex.ResourcePackLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui
{
	public class GuiRenderer : IGuiRenderer
	{
		private Alex Alex { get; }

		private IFont _font;

		public IFont Font
		{
			get => _font;
			set
			{
				_font = value;
				OnFontChanged();
			}
		}

		public GuiScaledResolution ScaledResolution { get; set; }

		public CultureLanguage Language =
			new CultureLanguage(CultureInfo.InstalledUICulture ?? CultureInfo.GetCultureInfo("en-US"));

		private GraphicsDevice  _graphicsDevice;
		private ResourceManager _resourceManager;

		private Dictionary<GuiTextures, TextureSlice2D> _textureCache = new Dictionary<GuiTextures, TextureSlice2D>();

		private Texture2D _widgets;
		private Texture2D _icons;
		private Texture2D _scrollbar;

		#region SpriteSheet Definitions

		#region Widgets

		private static readonly Rectangle WidgetHotBar                = new Rectangle(0, 0,  182, 22);
		private static readonly Rectangle WidgetHotBarSelectedOverlay = new Rectangle(0, 22, 24,  24);
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

		#region ScrollBar

		public static readonly Rectangle ScrollBarBackgroundDefault  = new Rectangle(0, 0, 10, 10);
		public static readonly Rectangle ScrollBarBackgroundHover    = new Rectangle(0, 0, 10, 10);
		public static readonly Rectangle ScrollBarBackgroundFocus    = new Rectangle(0, 0, 10, 10);
		public static readonly Rectangle ScrollBarBackgroundDisabled = new Rectangle(0, 0, 10, 10);

		public static readonly Rectangle ScrollBarTrackDefault  = new Rectangle(10, 10, 10, 10);
		public static readonly Rectangle ScrollBarTrackHover    = new Rectangle(10, 10, 10, 10);
		public static readonly Rectangle ScrollBarTrackFocus    = new Rectangle(10, 10, 10, 10);
		public static readonly Rectangle ScrollBarTrackDisabled = new Rectangle(10, 10, 10, 10);

		public static readonly Rectangle ScrollBarUpButtonDefault  = new Rectangle(20, 20, 10, 10);
		public static readonly Rectangle ScrollBarUpButtonHover    = new Rectangle(20, 20, 10, 10);
		public static readonly Rectangle ScrollBarUpButtonFocus    = new Rectangle(20, 20, 10, 10);
		public static readonly Rectangle ScrollBarUpButtonDisabled = new Rectangle(20, 20, 10, 10);

		public static readonly Rectangle ScrollBarDownButtonDefault  = new Rectangle(30, 30, 10, 10);
		public static readonly Rectangle ScrollBarDownButtonHover    = new Rectangle(30, 30, 10, 10);
		public static readonly Rectangle ScrollBarDownButtonFocus    = new Rectangle(30, 30, 10, 10);
		public static readonly Rectangle ScrollBarDownButtonDisabled = new Rectangle(30, 30, 10, 10);

		#endregion

		#endregion


		public GuiRenderer(Alex alex)
		{
			Alex = alex;
			Init(alex.GraphicsDevice);
		}


		public void Init(GraphicsDevice graphics)
		{
			_graphicsDevice  = graphics;
			_resourceManager = Alex.Resources;
			LoadEmbeddedTextures();

			if (_resourceManager?.ResourcePack != null)
			{
				LoadResourcePack(_resourceManager.ResourcePack);
			}
		}

		private void OnFontChanged()
		{
		}

		public void LoadResourcePack(McResourcePack resourcePack)
		{
			LoadLanguages(resourcePack);
			LoadResourcePackTextures(resourcePack);
		}

		public void SetLanguage(string cultureCode)
		{
			var matchingResults = _resourceManager.ResourcePack.Languages.Where(x => x.Value.CultureCode == cultureCode)
												  .Select(x => x.Value).ToArray();

			if (matchingResults.Length <= 0) return;
			CultureLanguage newLanguage = new CultureLanguage(CultureInfo.CreateSpecificCulture(cultureCode));

			foreach (var lang in matchingResults)
			{
				newLanguage.Load(lang);
			}
		}

		private void LoadLanguages(McResourcePack resourcePack)
		{
			Language.Load(resourcePack.Languages.FirstOrDefault().Value);
		}


		private void LoadEmbeddedTextures()
		{
			LoadTextureFromEmbeddedResource(GuiTextures.AlexLogo,
											ResourceManager.ReadResource("Alex.Resources.logo2.png"));
			LoadTextureFromEmbeddedResource(GuiTextures.ProgressBar,
											ResourceManager.ReadResource("Alex.Resources.ProgressBar.png"));
			LoadTextureFromEmbeddedResource(GuiTextures.SplashBackground,
											ResourceManager.ReadResource("Alex.Resources.Splash.png"));
		}


		private void LoadResourcePackTextures(McResourcePack resourcePack)
		{
			LoadTextureFromResourcePack(GuiTextures.AlexLogo, resourcePack, "");

			// First load Widgets
			resourcePack.TryGetTexture("gui/widgets", out _widgets);
			LoadWidgets(_widgets);

			resourcePack.TryGetTexture("gui/icons", out _icons);
			LoadIcons(_icons);

			_scrollbar = TextureUtils.ImageToTexture2D(_graphicsDevice, ResourceManager.ReadResource("Alex.Resources.ScrollBar.png"));
			LoadScrollBar(_scrollbar);

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
			LoadTextureFromSpriteSheet(GuiTextures.Inventory_HotBar, spriteSheet, WidgetHotBar);
			LoadTextureFromSpriteSheet(GuiTextures.Inventory_HotBar_SelectedItemOverlay, spriteSheet,
									   WidgetHotBarSelectedOverlay);

			LoadTextureFromSpriteSheet(GuiTextures.ButtonDefault,  spriteSheet, WidgetButtonDefault);
			LoadTextureFromSpriteSheet(GuiTextures.ButtonHover,    spriteSheet, WidgetButtonHover);
			LoadTextureFromSpriteSheet(GuiTextures.ButtonFocused,  spriteSheet, WidgetButtonHover);
			LoadTextureFromSpriteSheet(GuiTextures.ButtonDisabled, spriteSheet, WidgetButtonDisabled);
			LoadTextureFromSpriteSheet(GuiTextures.PanelGeneric, spriteSheet, WidgetHotBarSeparated,
									   new Thickness(5));
		}

		private void LoadIcons(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(GuiTextures.Crosshair,   spriteSheet, IconCrosshair);
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

		private void LoadScrollBar(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarBackground, spriteSheet, ScrollBarBackgroundDefault);

			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarTrackDefault,  spriteSheet, ScrollBarTrackDefault);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarTrackHover,    spriteSheet, ScrollBarTrackHover);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarTrackFocused,  spriteSheet, ScrollBarTrackFocus);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarTrackDisabled, spriteSheet, ScrollBarTrackDisabled);

			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarUpButtonDefault,  spriteSheet, ScrollBarUpButtonDefault);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarUpButtonHover,    spriteSheet, ScrollBarUpButtonHover);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarUpButtonFocused,  spriteSheet, ScrollBarUpButtonFocus);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarUpButtonDisabled, spriteSheet, ScrollBarUpButtonDisabled);

			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarDownButtonDefault,  spriteSheet, ScrollBarDownButtonDefault);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarDownButtonHover,    spriteSheet, ScrollBarDownButtonHover);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarDownButtonFocused,  spriteSheet, ScrollBarDownButtonFocus);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarDownButtonDisabled, spriteSheet, ScrollBarDownButtonDisabled);
		}

		private TextureSlice2D LoadTextureFromEmbeddedResource(GuiTextures guiTexture, byte[] resource)
		{
			_textureCache[guiTexture] = TextureUtils.ImageToTexture2D(_graphicsDevice, resource);
			return _textureCache[guiTexture];
		}

		private void LoadTextureFromResourcePack(GuiTextures guiTexture, McResourcePack resourcePack, string path,
												 float       scale = 1f)
		{
			if (resourcePack.TryGetTexture(path, out var texture))
			{
				_textureCache[guiTexture] = texture;
			}
		}

		private void LoadTextureFromSpriteSheet(GuiTextures guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle,
												Thickness   ninePatchThickness)
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

			return (TextureSlice2D) GpuResourceManager.GetTexture2D(_graphicsDevice, 1, 1);
		}

		public Texture2D GetTexture2D(GuiTextures guiTexture)
		{
			return GetTexture(guiTexture).Texture;
		}

		public string GetTranslation(string key)
		{
			return Language[key];
		}

		public Vector2 Project(Vector2 point)
		{
			return Vector2.Transform(point, ScaledResolution.TransformMatrix);
		}

		public Vector2 Unproject(Vector2 screen)
		{
			return Vector2.Transform(screen, ScaledResolution.InverseTransformMatrix);
		}
	}
}
