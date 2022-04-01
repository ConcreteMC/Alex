using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alex.Audio;
using Alex.Common.Gui.Graphics;
using Alex.Common.Localization;
using Alex.Common.Utils;
using Alex.Interfaces;
using Alex.Networking.Java.Models;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using RocketUI;
using RocketUI.Audio;
using RocketUI.Serialization.Xaml;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using AudioEngine = Alex.Audio.AudioEngine;
using Color = Microsoft.Xna.Framework.Color;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Size = RocketUI.Size;

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

		public CultureLanguage Language = new CultureLanguage();

		private GraphicsDevice _graphicsDevice;
		private ResourceManager _resourceManager;

		private readonly Dictionary<GuiTextures, TextureSlice2D> _textureCache =
			new Dictionary<GuiTextures, TextureSlice2D>();

		private readonly Dictionary<string, TextureSlice2D> _pathedTextureCache =
			new Dictionary<string, TextureSlice2D>();

		private readonly Dictionary<GuiSoundEffects, ISoundEffect> _soundEffectCache =
			new Dictionary<GuiSoundEffects, ISoundEffect>();

		private Texture2D _widgets;
		private Texture2D _icons;
		private Texture2D _bars;
		private Texture2D _scrollbar;
		private Texture2D _inventory;
		private Texture2D _chestInventory;
		private Texture2D _craftingTable;
		private Texture2D _furnace;
		private Texture2D _tabItemSearch;
		private Texture2D _mcLogo;
		private Texture2D _mapIcons;

		#region SpriteSheet Definitions

		#region Widgets

		private static readonly Rectangle WidgetHotBar = new Rectangle(0, 0, 182, 22);
		private static readonly Rectangle WidgetHotBarSelectedOverlay = new Rectangle(0, 22, 24, 24);
		private static readonly Rectangle WidgetButtonDisabled = new Rectangle(0, 46, 200, 20);
		private static readonly Rectangle WidgetButtonDefault = new Rectangle(0, 66, 200, 20);
		private static readonly Rectangle WidgetButtonHover = new Rectangle(0, 86, 200, 20);

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
		private static readonly Rectangle IconHalfHeart = new Rectangle(61, 0, 9, 9);

		#endregion

		#region ScrollBar

		public static readonly Rectangle ScrollBarBackgroundDefault = new Rectangle(0, 0, 10, 10);

		public static readonly Rectangle ScrollBarTrackDefault = new Rectangle(10, 10, 10, 10);
		public static readonly Rectangle ScrollBarTrackHover = new Rectangle(10, 10, 10, 10);
		public static readonly Rectangle ScrollBarTrackFocus = new Rectangle(10, 10, 10, 10);
		public static readonly Rectangle ScrollBarTrackDisabled = new Rectangle(10, 10, 10, 10);

		public static readonly Rectangle ScrollBarUpButtonDefault = new Rectangle(20, 20, 10, 10);
		public static readonly Rectangle ScrollBarUpButtonHover = new Rectangle(20, 20, 10, 10);
		public static readonly Rectangle ScrollBarUpButtonFocus = new Rectangle(20, 20, 10, 10);
		public static readonly Rectangle ScrollBarUpButtonDisabled = new Rectangle(20, 20, 10, 10);

		public static readonly Rectangle ScrollBarDownButtonDefault = new Rectangle(30, 30, 10, 10);
		public static readonly Rectangle ScrollBarDownButtonHover = new Rectangle(30, 30, 10, 10);
		public static readonly Rectangle ScrollBarDownButtonFocus = new Rectangle(30, 30, 10, 10);
		public static readonly Rectangle ScrollBarDownButtonDisabled = new Rectangle(30, 30, 10, 10);

		#endregion

		#endregion

		private StyleSheet _styleSheet;

		public GuiRenderer() { }


		public void Init(GraphicsDevice graphics, IServiceProvider serviceProvider)
		{
			_graphicsDevice = graphics;
			_resourceManager = serviceProvider.GetRequiredService<ResourceManager>();

			LoadEmbeddedTextures();
			LoadSoundEffects(serviceProvider.GetRequiredService<AudioEngine>());
			LoadStyleSheets();
		}

		private void LoadSoundEffects(AudioEngine audioEngine)
		{
			LoadSoundEffect(GuiSoundEffects.ButtonClick, new RocketSoundEffect(audioEngine, "random.click"));
		}

		private void LoadSoundEffect(GuiSoundEffects guiSoundEffects, ISoundEffect soundEffect)
		{
			_soundEffectCache[guiSoundEffects] = soundEffect;
		}

		public ISoundEffect GetSoundEffect(GuiSoundEffects soundEffects)
		{
			if (_soundEffectCache.TryGetValue(soundEffects, out var soundEffect))
			{
				return soundEffect; //.CreateInstance();
			}

			return null;
		}

		private void OnFontChanged() { }

		//private CultureInfo Culture { get; set; }
		public bool SetLanguage(string cultureCode)
		{
			//cultureCode = cultureCode;

			try
			{
				/*Culture = CultureInfo.GetCultureInfo(cultureCode.Replace("_", "-"));
				CultureInfo.CurrentCulture = Culture;
				CultureInfo.CurrentUICulture = Culture;
				CultureInfo.DefaultThreadCurrentUICulture = Culture;
				CultureInfo.DefaultThreadCurrentUICulture = Culture;
*/
				if (_languages.TryGetValue(cultureCode, out var lng)
				    || _languages.TryGetValue(cultureCode.Replace("-", "_"), out lng))
				{
					Language = lng;
					ChatParser.Language = lng;

					return true;
				}

				/*var matchingResults = _resourceManager.ResourcePack.Languages
				   .Where(x => x.Value.CultureCode == cultureCode).Select(x => x.Value).ToArray();

				if (matchingResults.Length <= 0) return false;
			//	var             cultureInfo = CultureInfo.GetCultureInfo(cultureCode.Replace("_", "-"));
				
				CultureLanguage newLanguage = new CultureLanguage();

				foreach (var lang in matchingResults)
				{
					newLanguage.Load(lang);
				}

				Language = newLanguage;

				return true;*/
			}
			catch (CultureNotFoundException) { }

			return false;
		}

		private readonly Dictionary<string, CultureLanguage> _languages = new Dictionary<string, CultureLanguage>();
		public IReadOnlyDictionary<string, CultureLanguage> Languages => _languages;

		public void LoadLanguages(MCJavaResourcePack resourcePack, IProgressReceiver progressReceiver)
		{
			if (resourcePack.Languages == null)
				return;

			var languages = resourcePack.Languages.Count;
			int done = 0;

			foreach (var lng in resourcePack.Languages)
			{
				if (lng.Value?.CultureCode == null)
					continue;

				var key = lng.Value.CultureCode.ToLower();

				progressReceiver?.UpdateProgress(done, languages, "Loading languages...", key);

				try
				{
					if (!_languages.TryGetValue(key, out var language))
					{
						language = new CultureLanguage()
						{
							Name = lng.Key.ToString(),
							Code = lng.Value.CultureCode,
							Region = lng.Value.CultureRegion
						};

						if (!string.IsNullOrWhiteSpace(lng.Value.CultureRegion))
						{
							language.DisplayName = $"{lng.Value.CultureName} ({lng.Value.CultureRegion})";
						}
					}

					if (language != null)
					{
						language.Load(lng.Value);
						_languages[key] = language;
					}
				}
				catch (CultureNotFoundException) { }
			}
		}


		private void LoadEmbeddedTextures()
		{
			//LoadTextureFromEmbeddedResource(AlexGuiTextures.AlexLogo, ResourceManager.ReadResource("Alex.Resources.logo2.png"));
			LoadTextureFromEmbeddedResource(
				AlexGuiTextures.ProgressBar, ResourceManager.ReadResource("Alex.Resources.ProgressBar.png"));

			LoadTextureFromEmbeddedResource(
				AlexGuiTextures.SplashBackground, ResourceManager.ReadResource("Alex.Resources.Splash.png"));

			LoadTextureFromEmbeddedResource(
				AlexGuiTextures.GradientBlur, ResourceManager.ReadResource("Alex.Resources.GradientBlur.png"));
		}

		public T GetGlobalOrDefault<T>(string variable, T defaultValue)
		{
			foreach (var resource in _resourceManager.ActiveBedrockResources.Reverse())
			{
				if (resource.GlobalUiVariables.TryGetValue(variable, out JToken v))
				{
					return v.ToObject<T>(MCJsonConvert.Serializer);
				}
			}

			return defaultValue;
		}

		public void LoadResourcePackTextures(ResourceManager resourceManager, IProgressReceiver progressReceiver)
		{
			//progressReceiver?.UpdateProgress(0, null, "gui/widgets");
			//LoadTextureFromResourcePack(GuiTextures.AlexLogo, resourcePack, "");

			_resourceManager = resourceManager;

			foreach (var color in TextColor.Colors)
			{
				var c = GetGlobalOrDefault($"${color.Code}_color_format", color.ForegroundColor.ToVector3());
				color.ForegroundColor = new Color(c);
			}

			progressReceiver?.UpdateProgress(0, null, "gui/title/minecraft");
			Image<Rgba32> mcBmp;

			if (resourceManager.TryGetBedrockBitmap("textures/ui/title", out mcBmp))
			{
				mcBmp.Mutate(x => x.Resize(275, 44));

				var oldLogo = _mcLogo;
				_mcLogo = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, mcBmp);
				oldLogo?.Dispose();

				LoadTextureFromSpriteSheet(
					AlexGuiTextures.AlexLogo, _mcLogo, new Rectangle(0, 0, 275, 44), new Size(275, 44));

				mcBmp.Dispose();
			}
			else if (resourceManager.TryGetBitmap("gui/title/minecraft", out mcBmp))
			{
				var part1 = mcBmp.Clone();
				part1.Mutate(x => x.Crop(new SixLabors.ImageSharp.Rectangle(0, 0, 154, 44)));

				var part2 = mcBmp.Clone();
				part2.Mutate(x => x.Crop(new SixLabors.ImageSharp.Rectangle(0, 46, 119, 44)));

				var finalLogo = new Image<Rgba32>(275, 44);

				finalLogo.Mutate(
					x =>
					{
						x.DrawImage(part1, new Point(1, 0), PixelColorBlendingMode.Normal, 1f);
						x.DrawImage(part2, new Point(156, 0), PixelColorBlendingMode.Normal, 1f);
					});

				_mcLogo = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, finalLogo);

				LoadTextureFromSpriteSheet(
					AlexGuiTextures.AlexLogo, _mcLogo, new Rectangle(0, 0, 273, 44), new Size(273, 44));

				mcBmp.Dispose();
				part1?.Dispose();
				part2?.Dispose();
			}

			// First load Widgets
			progressReceiver?.UpdateProgress(0, null, "gui/widgets");

			if (resourceManager.TryGetBitmap("gui/widgets", out var widgetsBmp))
			{
				var o = _widgets;
				_widgets = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, widgetsBmp);
				o?.Dispose();

				LoadWidgets(_widgets);
				widgetsBmp.Dispose();
			}

			progressReceiver?.UpdateProgress(25, null, "gui/icons");

			if (resourceManager.TryGetBitmap("gui/icons", out var icons))
			{
				var o = _icons;
				_icons = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, icons);
				o?.Dispose();
				LoadIcons(_icons);

				icons.Dispose();
			}

			if (resourceManager.TryGetBitmap("gui/bars", out var bars))
			{
				var o = _bars;
				_bars = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, bars);
				o?.Dispose();
				LoadBars(_bars);

				bars.Dispose();
			}

			if (resourceManager.TryGetBitmap("map/map_icons", out var mapIconsBmp))
			{
				var o = _mapIcons;
				_mapIcons = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, mapIconsBmp);
				o?.Dispose();

				LoadMapIcons(_mapIcons);
				mapIconsBmp.Dispose();
			}

			if (_scrollbar == null)
			{
				var o = _scrollbar;

				_scrollbar = TextureUtils.ImageToTexture2D(
					this, _graphicsDevice, ResourceManager.ReadResource("Alex.Resources.ScrollBar.png"));

				o?.Dispose();

				LoadScrollBar(_scrollbar);
			}

			// Backgrounds
			progressReceiver?.UpdateProgress(50, null, "gui/options_background");

			LoadTextureFromResourcePack(
				AlexGuiTextures.OptionsBackground, resourceManager, "gui/options_background", 2f);

			// Load Gui Containers
			{
				progressReceiver?.UpdateProgress(0, null, "gui/container/inventory");

				if (resourceManager.TryGetBitmap("gui/container/inventory", out var bmp))
				{
					var o = _inventory;
					_inventory = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, bmp);
					o?.Dispose();

					LoadTextureFromSpriteSheet(
						AlexGuiTextures.InventoryPlayerBackground, _inventory, new Rectangle(0, 0, 176, 166), IconSize);

					bmp.Dispose();
				}

				if (resourceManager.TryGetBitmap("gui/container/generic_54", out var genericInvBmp))
				{
					var o = _chestInventory;
					_chestInventory = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, genericInvBmp);
					o?.Dispose();

					LoadTextureFromSpriteSheet(
						AlexGuiTextures.InventoryChestBackground, _chestInventory, new Rectangle(0, 0, 175, 221),
						IconSize);

					genericInvBmp.Dispose();
				}

				if (resourceManager.TryGetBitmap("gui/container/crafting_table", out var craftingTable))
				{
					var o = _craftingTable;
					_craftingTable = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, craftingTable);
					o?.Dispose();

					LoadTextureFromSpriteSheet(
						AlexGuiTextures.InventoryCraftingTable, _craftingTable, new Rectangle(0, 0, 175, 165),
						IconSize);

					craftingTable.Dispose();
				}

				if (resourceManager.TryGetBitmap("gui/container/furnace", out var furnace))
				{
					var o = _furnace;
					_furnace = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, furnace);
					o?.Dispose();

					LoadTextureFromSpriteSheet(
						AlexGuiTextures.InventoryFurnace, _furnace, new Rectangle(0, 0, 175, 165), IconSize);

					furnace.Dispose();
				}

				if (resourceManager.TryGetBitmap("gui/container/creative_inventory/tab_item_search", out var tabImage))
				{
					var o = _tabItemSearch;
					_tabItemSearch = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, tabImage);
					o?.Dispose();

					LoadTextureFromSpriteSheet(
						AlexGuiTextures.InventoryCreativeItemSearch, _tabItemSearch, new Rectangle(0, 0, 194, 135),
						IconSize);

					tabImage.Dispose();
				}
				//LoadTextureFromSpriteSheet(GuiTextures.InventoryChestBackground, _inventory, new Rectangle(0, 0, 175, 221), IconSize);
			}

			progressReceiver?.UpdateProgress(75, null, "gui/title/background");

			// Panorama
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama0, resourceManager, "gui/title/background/panorama_0");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama1, resourceManager, "gui/title/background/panorama_1");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama2, resourceManager, "gui/title/background/panorama_2");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama3, resourceManager, "gui/title/background/panorama_3");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama4, resourceManager, "gui/title/background/panorama_4");
			LoadTextureFromResourcePack(AlexGuiTextures.Panorama5, resourceManager, "gui/title/background/panorama_5");

			// Other
			LoadTextureFromResourcePack(AlexGuiTextures.DefaultServerIcon, resourceManager, "misc/unknown_server");

			LoadTextureFromResourcePack(AlexGuiTextures.UnknownPackIcon, resourceManager, "misc/unknown_pack");

			Extensions.InitMarkers(this);

			progressReceiver?.UpdateProgress(100, null, "");
		}

		private void LoadMapIcons(Texture2D spriteSheet)
		{
			var iconSize = new Size(128, 128);

			int x = 0;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.WhitePointer, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.GreenPointer, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.RedPointer, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.BluePointer, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.Cross, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.RedThing, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.BigDot, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.SmallDot, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.House, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.BlueStructure, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.WhiteBanner, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.OrangeBanner, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.PurpleBanner, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.LightBlueBanner, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.YellowBanner, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.LightGreenBanner, spriteSheet, new Rectangle(x, 0, 8, 8), iconSize);

			//2nd row
			x = 0;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.PinkBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.DarkGrayBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.GrayBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.CyanBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.DarkPurpleBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.BlueBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize); //TODO

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.BrownBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.GreenBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.RedBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.BlackBanner, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);

			x += 8;

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.MapMarkers.RedCross, spriteSheet, new Rectangle(x, 8, 8, 8), iconSize);
		}

		private void LoadWidgets(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(AlexGuiTextures.Inventory_HotBar, spriteSheet, WidgetHotBar, IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.Inventory_HotBar_SelectedItemOverlay, spriteSheet, WidgetHotBarSelectedOverlay,
				IconSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.ButtonDefault, spriteSheet, WidgetButtonDefault, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ButtonHover, spriteSheet, WidgetButtonHover, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ButtonFocused, spriteSheet, WidgetButtonHover, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ButtonDisabled, spriteSheet, WidgetButtonDisabled, IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.PanelGeneric, spriteSheet, WidgetHotBarSeparated, new Thickness(5), IconSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.GreenCheckMark, spriteSheet, WidgetGreen, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.GreyCheckMark, spriteSheet, WidgetGrey, IconSize);
		}

		private Size IconSize { get; } = new Size(256, 256);

		private void LoadIcons(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(AlexGuiTextures.Crosshair, spriteSheet, IconCrosshair, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing0, spriteSheet, IconServerPing0, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing1, spriteSheet, IconServerPing1, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing2, spriteSheet, IconServerPing2, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing3, spriteSheet, IconServerPing3, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing4, spriteSheet, IconServerPing4, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.ServerPing5, spriteSheet, IconServerPing5, IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.ServerPingPending1, spriteSheet, IconServerPingPending1, IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.ServerPingPending2, spriteSheet, IconServerPingPending2, IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.ServerPingPending3, spriteSheet, IconServerPingPending3, IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.ServerPingPending4, spriteSheet, IconServerPingPending4, IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.ServerPingPending5, spriteSheet, IconServerPingPending5, IconSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.HealthPlaceholder, spriteSheet, IconHeartHolder, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.HealthHeart, spriteSheet, IconHeart, IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.HealthHalfHeart, spriteSheet, IconHalfHeart, IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.HungerPlaceholder, spriteSheet, new Rectangle(16, 27, 9, 9), IconSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.HungerFull, spriteSheet, new Rectangle(52, 27, 9, 9), IconSize);
			LoadTextureFromSpriteSheet(AlexGuiTextures.HungerHalf, spriteSheet, new Rectangle(61, 27, 9, 9), IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.ExperienceBackground, spriteSheet, new Rectangle(0, 64, 182, 5), IconSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.Experience, spriteSheet, new Rectangle(0, 69, 182, 5), IconSize);

			LoadTextureFromSpriteSheet(AlexGuiTextures.AirBubble, spriteSheet, new Rectangle(16, 18, 9, 9), IconSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.PoppedAirBubble, spriteSheet, new Rectangle(25, 18, 9, 9), IconSize);
		}

		private Size BarsSize { get; } = new Size(256, 256);

		private void LoadBars(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarBackgroundPink, spriteSheet, new Rectangle(0, 0, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarProgressPink, spriteSheet, new Rectangle(0, 5, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarBackgroundBlue, spriteSheet, new Rectangle(0, 10, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarProgressBlue, spriteSheet, new Rectangle(0, 15, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarBackgroundRed, spriteSheet, new Rectangle(0, 20, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarProgressRed, spriteSheet, new Rectangle(0, 25, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarBackgroundGreen, spriteSheet, new Rectangle(0, 30, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarProgressGreen, spriteSheet, new Rectangle(0, 35, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarBackgroundYellow, spriteSheet, new Rectangle(0, 40, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarProgressYellow, spriteSheet, new Rectangle(0, 45, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarBackgroundPurple, spriteSheet, new Rectangle(0, 50, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarProgressPurple, spriteSheet, new Rectangle(0, 55, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarBackgroundWhite, spriteSheet, new Rectangle(0, 60, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarProgressWhite, spriteSheet, new Rectangle(0, 65, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarDivider6, spriteSheet, new Rectangle(0, 80, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarDivider10, spriteSheet, new Rectangle(0, 90, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarDivider12, spriteSheet, new Rectangle(0, 100, 182, 5), BarsSize);

			LoadTextureFromSpriteSheet(
				AlexGuiTextures.BossbarDivider20, spriteSheet, new Rectangle(0, 110, 182, 5), BarsSize);
		}

		private Size ScrollbarSize { get; } = new Size(40, 40);

		private void LoadScrollBar(Texture2D spriteSheet)
		{
			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarBackground, spriteSheet, ScrollBarBackgroundDefault, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarTrackDefault, spriteSheet, ScrollBarTrackDefault, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarTrackHover, spriteSheet, ScrollBarTrackHover, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarTrackFocused, spriteSheet, ScrollBarTrackFocus, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarTrackDisabled, spriteSheet, ScrollBarTrackDisabled, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarUpButtonDefault, spriteSheet, ScrollBarUpButtonDefault, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarUpButtonHover, spriteSheet, ScrollBarUpButtonHover, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarUpButtonFocused, spriteSheet, ScrollBarUpButtonFocus, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarUpButtonDisabled, spriteSheet, ScrollBarUpButtonDisabled, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarDownButtonDefault, spriteSheet, ScrollBarDownButtonDefault, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarDownButtonHover, spriteSheet, ScrollBarDownButtonHover, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarDownButtonFocused, spriteSheet, ScrollBarDownButtonFocus, ScrollbarSize);

			LoadTextureFromSpriteSheet(
				GuiTextures.ScrollBarDownButtonDisabled, spriteSheet, ScrollBarDownButtonDisabled, ScrollbarSize);
		}


		private TextureSlice2D LoadTextureFromEmbeddedResource(GuiTextures guiTexture, byte[] resource)
		{
			_textureCache[guiTexture] = TextureUtils.ImageToTexture2D(this, _graphicsDevice, resource);

			return _textureCache[guiTexture];
		}

		private void LoadTextureFromResourcePack(GuiTextures guiTexture,
			ResourceManager resources,
			string path,
			float scale = 1f)
		{
			if (resources.TryGetBitmap(path, out var texture))
			{
				TextureSlice2D orig = null;
				_textureCache.TryGetValue(guiTexture, out orig);
				_textureCache[guiTexture] = TextureUtils.BitmapToTexture2D(this, _graphicsDevice, texture);
				orig?.Texture?.Dispose();

				texture.Dispose();
			}
		}

		private void LoadTextureFromSpriteSheet(GuiTextures guiTexture,
			Texture2D spriteSheet,
			Rectangle sliceRectangle,
			Thickness ninePatchThickness,
			Size originalSize)
		{
			var widthScaler = spriteSheet.Width / originalSize.Width;
			var heightScaler = spriteSheet.Height / originalSize.Height;

			_textureCache.TryGetValue(guiTexture, out var o);

			_textureCache[guiTexture] = new NinePatchTexture2D(
				spriteSheet.Slice(
					new Rectangle(
						sliceRectangle.X * widthScaler, sliceRectangle.Y * heightScaler,
						sliceRectangle.Width * widthScaler, sliceRectangle.Height * heightScaler)), ninePatchThickness);

			o?.Texture?.Dispose();
		}

		private void LoadTextureFromSpriteSheet(GuiTextures guiTexture,
			Texture2D spriteSheet,
			Rectangle sliceRectangle,
			Size originalSize)
		{
			var widthScaler = spriteSheet.Width / originalSize.Width;
			var heightScaler = spriteSheet.Height / originalSize.Height;

			_textureCache.TryGetValue(guiTexture, out var o);

			var slice = spriteSheet.Slice(
				new Rectangle(
					sliceRectangle.X * widthScaler, sliceRectangle.Y * heightScaler, sliceRectangle.Width * widthScaler,
					sliceRectangle.Height * heightScaler));

			_textureCache[guiTexture] = slice;

			o?.Texture?.Dispose();
		}

		public TextureSlice2D GetTexture(GuiTextures guiTexture)
		{
			if (_textureCache.TryGetValue(guiTexture, out var texture))
			{
				return texture;
			}

			return (TextureSlice2D)RocketUI.GpuResourceManager.CreateTexture2D(1, 1);
		}

		public TextureSlice2D GetTexture(string texturePath)
		{
			texturePath = texturePath.ToLowerInvariant();

			if (!_pathedTextureCache.TryGetValue(texturePath, out TextureSlice2D texture))
			{
				texture = Texture2D.FromFile(_graphicsDevice, texturePath);
				_pathedTextureCache.Add(texturePath, texture);
			}

			return texture;
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

		public GraphicsContext CreateGuiSpriteBatchContext(GraphicsDevice graphics)
		{
			return GraphicsContext.CreateContext(
				graphics, BlendState.NonPremultiplied, DepthStencilState.None,
				GuiSpriteBatch.GetDefaultRasterizerState(), SamplerState.PointClamp);
		}

		private void LoadStyleSheets()
		{
			_styleSheet = new StyleSheet();
			RocketXamlLoader.Load<StyleSheet>(_styleSheet, "Alex.Gui.UIStyles.xaml");
		}

		public IStyle[] ResolveStyles(Type elementType, string[] classNames)
		{
			return _styleSheet.ResolveStyles(elementType, classNames);

			/*if (elementType.IsAssignableFrom(typeof(StackMenuItem)))
			{
				return new[]
				{
					new Style()
					{
						Name = nameof(StackMenuItem),
						TargetType = typeof(StackMenuItem),
						Setters = new ObservableCollection<Setter>()
						{
							new Setter(
								nameof(Button.Background),
								new GuiTexture2D() { Color = Color.Transparent }),
							new Setter(
								nameof(Button.DisabledBackground),
								new GuiTexture2D() { Color = Color.Transparent }),
							new Setter(
								nameof(Button.FocusedBackground),
								new GuiTexture2D() { Color = Color.Transparent }),
							new Setter(
								nameof(Button.HighlightedBackground),
								new GuiTexture2D() { Color = new Color(Color.Black * 0.8f, 0.5f) }),
							new Setter(
								nameof(Button.HighlightColor),
								new GuiTexture2D() { Color = (Color)TextColor.Cyan }),
							new Setter(
								nameof(Button.DefaultColor),
								new GuiTexture2D() { Color = (Color)TextColor.White })
						},
					}
				};
			}
			else if (elementType.IsAssignableFrom(typeof(Button)))
			{
				return new[]
				{
					new Style()
					{
						Name = nameof(Button),
						TargetType = typeof(Button),
						Setters = new ObservableCollection<Setter>()
						{
							new Setter(
								nameof(Button.Background),
								new GuiTexture2D() { TextureResource = AlexGuiTextures.ButtonDefault })
						}
					}
				};
			}

			return null;*/
		}
	}
}