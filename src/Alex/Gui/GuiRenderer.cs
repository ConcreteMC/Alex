using System;
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
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;
using Size = System.Drawing.Size;

namespace Alex.Gui
{
	public class GuiRenderer : IGuiRenderer
	{
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
		private Texture2D _inventory;
		private Texture2D _chestInventory;
		private Texture2D _craftingTable;
		private Texture2D _furnace;
		private Texture2D _tabItemSearch;
		
		#region SpriteSheet Definitions

		#region Widgets

		private static readonly Rectangle WidgetHotBar                = new Rectangle(0, 0,  182, 22);
		private static readonly Rectangle WidgetHotBarSelectedOverlay = new Rectangle(0, 22, 24,  24);
		private static readonly Rectangle WidgetButtonDisabled        = new Rectangle(0, 46, 200, 20);
		private static readonly Rectangle WidgetButtonDefault         = new Rectangle(0, 66, 200, 20);
		private static readonly Rectangle WidgetButtonHover           = new Rectangle(0, 86, 200, 20);

		private static readonly Rectangle WidgetHotBarSeparated = new Rectangle(24, 23, 22, 22);

		private static readonly Rectangle WidgetGreen = new Rectangle(208, 0, 15, 15);
		private static readonly Rectangle WidgetGrey = new Rectangle(224, 0, 15, 15);
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

		private static readonly Rectangle IconHeartHolder = new Rectangle(16, 0, 9, 9);
		private static readonly Rectangle IconHeart = new Rectangle(52, 0, 9, 9);
		private static readonly Rectangle IconHalfHeart = new Rectangle(69, 0, 9, 9);
		
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


		public GuiRenderer()
		{
			
		}


		public void Init(GraphicsDevice graphics, IServiceProvider serviceProvider)
		{
			_graphicsDevice  = graphics;
			_resourceManager = serviceProvider.GetRequiredService<ResourceManager>();
			LoadEmbeddedTextures();

			var resourcePack = _resourceManager?.ResourcePack;
			if (resourcePack != null)
			{
				//LoadResourcePack(resourcePack, null);
			}
		}

		private void OnFontChanged()
		{
		}

		public void LoadResourcePack(McResourcePack resourcePack, IProgressReceiver progressReceiver)
		{
			LoadLanguages(resourcePack, progressReceiver);
			LoadResourcePackTextures(resourcePack, progressReceiver);
		}

		private CultureInfo Culture { get; set; } = CultureInfo.CreateSpecificCulture("en_us");
		public void SetLanguage(string cultureCode)
		{
			try
			{
				Culture = CultureInfo.GetCultureInfo(cultureCode);
				CultureInfo.CurrentCulture = Culture;
				CultureInfo.CurrentUICulture = Culture;
				CultureInfo.DefaultThreadCurrentUICulture = Culture;
				CultureInfo.DefaultThreadCurrentUICulture = Culture;

				if (_languages.TryGetValue(cultureCode, out var lng))
				{
					Language = lng;
					return;
				}

				if (_resourceManager.ResourcePack == null)
					return;

				var matchingResults = _resourceManager.ResourcePack.Languages
					.Where(x => x.Value.CultureCode == cultureCode)
					.Select(x => x.Value).ToArray();

				if (matchingResults.Length <= 0) return;
				CultureLanguage newLanguage = new CultureLanguage(Culture);

				foreach (var lang in matchingResults)
				{
					newLanguage.Load(lang);
				}

				Language = newLanguage;
			}catch(CultureNotFoundException){}
		}

		private Dictionary<string, CultureLanguage> _languages = new Dictionary<string, CultureLanguage>();
		public IReadOnlyDictionary<string, CultureLanguage> Languages => _languages;
		
		public void LoadLanguages(McResourcePack resourcePack, IProgressReceiver progressReceiver)
		{
			if (resourcePack.Languages == null)
				return;

			foreach (var lng in resourcePack.Languages)
			{
				try
				{
					var key = lng.Key.Split(':')[1];

					CultureLanguage language;
					if (!_languages.TryGetValue(key, out language))
					{
						language = new CultureLanguage(CultureInfo.GetCultureInfo(key));
					}

					//if (lng.Value.CultureCode == Culture.Name)
					language.Load(lng.Value);

					_languages[key] = language;
				}catch(CultureNotFoundException){}
			}
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


		public void LoadResourcePackTextures(McResourcePack resourcePack, IProgressReceiver progressReceiver)
		{
			//progressReceiver?.UpdateProgress(0, null, "gui/widgets");
			//LoadTextureFromResourcePack(GuiTextures.AlexLogo, resourcePack, "");

			// First load Widgets
			progressReceiver?.UpdateProgress(0, null, "gui/widgets");
			if (resourcePack.TryGetBitmap("gui/widgets", out var widgetsBmp))
			{
				_widgets = TextureUtils.BitmapToTexture2D(_graphicsDevice, widgetsBmp);
				LoadWidgets(_widgets);
			}

			progressReceiver?.UpdateProgress(25, null, "gui/icons");
			if (resourcePack.TryGetBitmap("gui/icons", out var icons))
			{
				_icons = TextureUtils.BitmapToTexture2D(_graphicsDevice, icons);
				LoadIcons(_icons);
			}

			if (_scrollbar == null)
			{
				_scrollbar = TextureUtils.ImageToTexture2D(_graphicsDevice,
					ResourceManager.ReadResource("Alex.Resources.ScrollBar.png"));
				LoadScrollBar(_scrollbar);
			}

			// Backgrounds
			progressReceiver?.UpdateProgress(50, null, "gui/options_background");
			LoadTextureFromResourcePack(GuiTextures.OptionsBackground, resourcePack, "gui/options_background", 2f);

			// Load Gui Containers
			{
				progressReceiver?.UpdateProgress(0, null, "gui/container/inventory");
				
				if (resourcePack.TryGetBitmap("gui/container/inventory", out var bmp))
				{
					_inventory = TextureUtils.BitmapToTexture2D(_graphicsDevice, bmp);
					LoadTextureFromSpriteSheet(GuiTextures.InventoryPlayerBackground, _inventory, new Rectangle(0, 0, 176, 166), IconSize);
				}

				if (resourcePack.TryGetBitmap("gui/container/generic_54", out var genericInvBmp))
				{
					_chestInventory = TextureUtils.BitmapToTexture2D(_graphicsDevice, genericInvBmp);
					LoadTextureFromSpriteSheet(GuiTextures.InventoryChestBackground, _chestInventory, new Rectangle(0, 0, 175, 221), IconSize);
				}

				if (resourcePack.TryGetBitmap("gui/container/crafting_table", out var craftingTable))
				{
					_craftingTable = TextureUtils.BitmapToTexture2D(_graphicsDevice, craftingTable);
					LoadTextureFromSpriteSheet(GuiTextures.InventoryCraftingTable, _craftingTable, new Rectangle(0, 0, 175, 165), IconSize);
				}
				
				if (resourcePack.TryGetBitmap("gui/container/furnace", out var furnace))
				{
					_furnace = TextureUtils.BitmapToTexture2D(_graphicsDevice, furnace);
					LoadTextureFromSpriteSheet(GuiTextures.InventoryFurnace, _furnace, new Rectangle(0, 0, 175, 165), IconSize);
				}

				if (resourcePack.TryGetBitmap("gui/container/creative_inventory/tab_item_search", out var tabImage))
				{
					_tabItemSearch = TextureUtils.BitmapToTexture2D(_graphicsDevice, tabImage);
					LoadTextureFromSpriteSheet(GuiTextures.InventoryCreativeItemSearch, _tabItemSearch, new Rectangle(0, 0, 194, 135), IconSize);
				}
				//LoadTextureFromSpriteSheet(GuiTextures.InventoryChestBackground, _inventory, new Rectangle(0, 0, 175, 221), IconSize);
			}

			progressReceiver?.UpdateProgress(75, null, "gui/title/background");
			
			// Panorama
			LoadTextureFromResourcePack(GuiTextures.Panorama0, resourcePack, "gui/title/background/panorama_0");
			LoadTextureFromResourcePack(GuiTextures.Panorama1, resourcePack, "gui/title/background/panorama_1");
			LoadTextureFromResourcePack(GuiTextures.Panorama2, resourcePack, "gui/title/background/panorama_2");
			LoadTextureFromResourcePack(GuiTextures.Panorama3, resourcePack, "gui/title/background/panorama_3");
			LoadTextureFromResourcePack(GuiTextures.Panorama4, resourcePack, "gui/title/background/panorama_4");
			LoadTextureFromResourcePack(GuiTextures.Panorama5, resourcePack, "gui/title/background/panorama_5");

			// Other
			LoadTextureFromResourcePack(GuiTextures.DefaultServerIcon, resourcePack, "misc/unknown_server");
			
			progressReceiver?.UpdateProgress(100, null, "");
		}

		private void LoadWidgets(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(GuiTextures.Inventory_HotBar, spriteSheet, WidgetHotBar, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.Inventory_HotBar_SelectedItemOverlay, spriteSheet,
									   WidgetHotBarSelectedOverlay, IconSize);

			LoadTextureFromSpriteSheet(GuiTextures.ButtonDefault,  spriteSheet, WidgetButtonDefault, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ButtonHover,    spriteSheet, WidgetButtonHover, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ButtonFocused,  spriteSheet, WidgetButtonHover, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ButtonDisabled, spriteSheet, WidgetButtonDisabled, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.PanelGeneric, spriteSheet, WidgetHotBarSeparated,
									   new Thickness(5), IconSize);
			
			LoadTextureFromSpriteSheet(GuiTextures.GreenCheckMark, spriteSheet, WidgetGreen, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.GreyCheckMark, spriteSheet, WidgetGrey, IconSize);
		}

		private Size IconSize { get; } = new Size(256, 256);
		private void LoadIcons(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(GuiTextures.Crosshair,   spriteSheet, IconCrosshair, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPing0, spriteSheet, IconServerPing0, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPing1, spriteSheet, IconServerPing1, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPing2, spriteSheet, IconServerPing2, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPing3, spriteSheet, IconServerPing3, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPing4, spriteSheet, IconServerPing4, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPing5, spriteSheet, IconServerPing5, IconSize);

			LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending1, spriteSheet, IconServerPingPending1, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending2, spriteSheet, IconServerPingPending2, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending3, spriteSheet, IconServerPingPending3, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending4, spriteSheet, IconServerPingPending4, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.ServerPingPending5, spriteSheet, IconServerPingPending5, IconSize);
			
			LoadTextureFromSpriteSheet(GuiTextures.HealthPlaceholder, spriteSheet, IconHeartHolder, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.HealthHeart, spriteSheet, IconHeart, IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.HealthHalfHeart, spriteSheet, IconHalfHeart, IconSize);
			
			LoadTextureFromSpriteSheet(GuiTextures.HungerPlaceholder, spriteSheet, new Rectangle(16, 27, 9, 9), IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.HungerFull, spriteSheet, new Rectangle(52, 27, 9, 9), IconSize);
			LoadTextureFromSpriteSheet(GuiTextures.HungerHalf, spriteSheet, new Rectangle(61, 27, 9, 9), IconSize);
		}

		private Size ScrollbarSize { get; } = new Size(40,40);
		private void LoadScrollBar(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarBackground, spriteSheet, ScrollBarBackgroundDefault, ScrollbarSize);

			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarTrackDefault,  spriteSheet, ScrollBarTrackDefault, ScrollbarSize);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarTrackHover,    spriteSheet, ScrollBarTrackHover, ScrollbarSize);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarTrackFocused,  spriteSheet, ScrollBarTrackFocus, ScrollbarSize);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarTrackDisabled, spriteSheet, ScrollBarTrackDisabled, ScrollbarSize);

			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarUpButtonDefault,  spriteSheet, ScrollBarUpButtonDefault, ScrollbarSize);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarUpButtonHover,    spriteSheet, ScrollBarUpButtonHover, ScrollbarSize);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarUpButtonFocused,  spriteSheet, ScrollBarUpButtonFocus, ScrollbarSize);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarUpButtonDisabled, spriteSheet, ScrollBarUpButtonDisabled, ScrollbarSize);

			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarDownButtonDefault,  spriteSheet, ScrollBarDownButtonDefault, ScrollbarSize);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarDownButtonHover,    spriteSheet, ScrollBarDownButtonHover, ScrollbarSize);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarDownButtonFocused,  spriteSheet, ScrollBarDownButtonFocus, ScrollbarSize);
			LoadTextureFromSpriteSheet(GuiTextures.ScrollBarDownButtonDisabled, spriteSheet, ScrollBarDownButtonDisabled, ScrollbarSize);
		}


		private TextureSlice2D LoadTextureFromEmbeddedResource(GuiTextures guiTexture, byte[] resource)
		{
			_textureCache[guiTexture] = TextureUtils.ImageToTexture2D(_graphicsDevice, resource);
			return _textureCache[guiTexture];
		}

		private void LoadTextureFromResourcePack(GuiTextures guiTexture, McResourcePack resourcePack, string path,
												 float       scale = 1f)
		{
			if (resourcePack.TryGetBitmap(path, out var texture))
			{
				_textureCache[guiTexture] = TextureUtils.BitmapToTexture2D(_graphicsDevice, texture);
			}
		}

		private void LoadTextureFromSpriteSheet(GuiTextures guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle,
												Thickness   ninePatchThickness, Size originalSize)
		{
			var widthScaler = spriteSheet.Width / originalSize.Width;
			var heightScaler = spriteSheet.Height / originalSize.Height;
			
			_textureCache[guiTexture] = new NinePatchTexture2D(spriteSheet.Slice(new Rectangle(sliceRectangle.X * widthScaler,
				sliceRectangle.Y * heightScaler, sliceRectangle.Width * widthScaler,
				sliceRectangle.Height * heightScaler)), ninePatchThickness);
		}

		private void LoadTextureFromSpriteSheet(GuiTextures guiTexture, Texture2D spriteSheet, Rectangle sliceRectangle, Size originalSize)
		{
			var widthScaler = spriteSheet.Width / originalSize.Width;
			var heightScaler = spriteSheet.Height / originalSize.Height;

			_textureCache[guiTexture] = spriteSheet.Slice(new Rectangle(sliceRectangle.X * widthScaler,
				sliceRectangle.Y * heightScaler, sliceRectangle.Width * widthScaler,
				sliceRectangle.Height * heightScaler));
		}
		
		public TextureSlice2D GetTexture(GuiTextures guiTexture)
		{
			if (_textureCache.TryGetValue(guiTexture, out var texture))
			{
				return texture;
			}

			return (TextureSlice2D) GpuResourceManager.GetTexture2D(this, _graphicsDevice, 1, 1);
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
