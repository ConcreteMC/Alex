namespace Alex.GuiDebugger.Markup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Markup;
    using System.Windows.Media;
    using Catel;
    using Catel.Logging;
    using Catel.Windows.Markup;
    using Orchestra;
    using Brush = System.Windows.Media.Brush;
    using FontFamily = System.Windows.Media.FontFamily;
    using FontStyle = System.Windows.FontStyle;
    using Point = System.Windows.Point;

    internal enum ResourceKeyID
    {
        FontImageGlyphRunDrawing
    }

    /// <summary>
    /// Markup extension that can show a font as image.
    /// </summary>
    /// <remarks>
    /// Original idea comes from http://www.codeproject.com/Tips/634540/Using-Font-Icons
    /// </remarks>
    public class FontImage : UpdatableMarkupExtension
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, FontFamily> RegisteredFontFamilies = new Dictionary<string, FontFamily>();
        private static readonly Double RenderingEmSize;

        
        #region Constructors
        static FontImage()
        {
            DefaultFontFamily = "Segoe UI Symbol";
            DefaultBrush = Brushes.Black;

            var dpi = ScreenHelper.GetDpi().Width;
            RenderingEmSize = dpi / 96d;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontImage"/> class.
        /// </summary>
        public FontImage()
        {
            AllowUpdatableStyleSetters = true;
            FontFamily = DefaultFontFamily;
            Brush = DefaultBrush;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontImage" /> class.
        /// </summary>
        /// <param name="itemName">Name of the resource.</param>
        public FontImage(string itemName)
            : this()
        {
            ItemName = itemName;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the default name of the font.
        /// </summary>
        /// <value>The default name of the font.</value>
        public static string DefaultFontFamily { get; set; }


        /// <summary>
        /// Gets or sets the default brush.
        /// </summary>
        /// <value>The default brush.</value>
        public static Brush DefaultBrush { get; set; }

        /// <summary>
        /// Gets the font family.
        /// </summary>
        /// <value>The font family.</value>
        public string FontFamily { get; set; }

        /// <summary>
        /// Gets or sets the brush.
        /// </summary>
        /// <value>The brush.</value>
        public Brush Brush { get; set; }
        
        /// <summary>
        /// Gets or sets the font item name.
        /// </summary>
        /// <value>The font item name.</value>
        [ConstructorArgument("itemName")]
        public string ItemName { get; set; }
        #endregion

        protected override object ProvideDynamicValue(IServiceProvider serviceProvider)
        {
            return GetImageSource();
        }

        public ImageSource GetImageSource()
        {
            if (CatelEnvironment.IsInDesignMode)
            {
                //return null;
            }

            var fontFamily = FontFamily;
            if (fontFamily == null)
            {
                throw Log.ErrorAndCreateException<InvalidOperationException>("FontFamily cannot be null, make sure to set it or use the DefaultFontFamily");
            }

            if (!RegisteredFontFamilies.ContainsKey(fontFamily))
            {
                throw Log.ErrorAndCreateException<InvalidOperationException>("FontFamily '{0}' is not yet registered, register it first using the RegisterFont method", fontFamily);
            }

            var family = RegisteredFontFamilies[fontFamily];

            // TODO: Consider caching
            var glyph = CreateGlyph(ItemName, family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal, Brush);
            return glyph;
        }

        public static ImageSource GetImageSource(string fontFamily, string text, Brush fillBrush)
        {
            if (CatelEnvironment.IsInDesignMode)
            {
                //return null;
            }

            if (fontFamily == null)
                fontFamily = DefaultFontFamily;

            if (fillBrush == null)
                fillBrush = DefaultBrush;

            if (fontFamily == null)
            {
                throw Log.ErrorAndCreateException<InvalidOperationException>("FontFamily cannot be null, make sure to set it or use the DefaultFontFamily");
            }

            if (!RegisteredFontFamilies.ContainsKey(fontFamily))
            {
                throw Log.ErrorAndCreateException<InvalidOperationException>("FontFamily '{0}' is not yet registered, register it first using the RegisterFont method", fontFamily);
            }

            var family = RegisteredFontFamilies[fontFamily];

            // TODO: Consider caching
            var glyph = CreateGlyph(text, family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal, fillBrush);
            return glyph;
        }
        
        public static GlyphRun GetGlyphRun(string fontFamily, string text)
        {
            if (CatelEnvironment.IsInDesignMode)
            {
                //return null;
            }

            if (fontFamily == null)
                fontFamily = DefaultFontFamily;

            if (fontFamily == null)
            {
                throw Log.ErrorAndCreateException<InvalidOperationException>("FontFamily cannot be null, make sure to set it or use the DefaultFontFamily");
            }

            if (!RegisteredFontFamilies.ContainsKey(fontFamily))
            {
                throw Log.ErrorAndCreateException<InvalidOperationException>("FontFamily '{0}' is not yet registered, register it first using the RegisterFont method", fontFamily);
            }

            var family = RegisteredFontFamilies[fontFamily];

            // TODO: Consider caching
            var glyph = GetGlyphRun(text, family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            return glyph;
        }

        private static ImageSource CreateGlyph(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, Brush foreBrush)
        {
            if (fontFamily != null && !string.IsNullOrEmpty(text))
            {
                var typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);

                GlyphTypeface glyphTypeface;
                if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                {
                    throw Log.ErrorAndCreateException<InvalidOperationException>("No glyph type face found");
                }

                var glyphIndexes = new ushort[text.Length];
                var advanceWidths = new double[text.Length];

                for (var i = 0; i < text.Length; i++)
                {
                    ushort glyphIndex;

                    try
                    {
                        var key = text[i];

                        if (!glyphTypeface.CharacterToGlyphMap.TryGetValue(key, out glyphIndex))
                        {
                            glyphIndex = 42;
                        }
                    }
                    catch (Exception)
                    {
                        glyphIndex = 42;
                    }

                    glyphIndexes[i] = glyphIndex;

                    var width = glyphTypeface.AdvanceWidths[glyphIndex] * 1.0;
                    advanceWidths[i] = width;
                }

                try
                {
#pragma warning disable 618
                    var glyphRun = new GlyphRun(glyphTypeface, 0, false, RenderingEmSize, glyphIndexes, new Point(0, 0), advanceWidths, null, null, null, null, null, null);
#pragma warning restore 618
                    var glyphRunDrawing = new GlyphRunDrawing(foreBrush, glyphRun);

                    var drawingImage = new DrawingImage(glyphRunDrawing);
                    return drawingImage;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in generating Glyphrun");
                }
            }

            return null;
        }

        private static GlyphRun GetGlyphRun(string text, FontFamily fontFamily, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch)
        {
            if (fontFamily != null && !string.IsNullOrEmpty(text))
            {
                var typeface = new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);

                GlyphTypeface glyphTypeface;
                if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                {
                    throw Log.ErrorAndCreateException<InvalidOperationException>("No glyph type face found");
                }

                var glyphIndexes = new ushort[text.Length];
                var advanceWidths = new double[text.Length];

                for (var i = 0; i < text.Length; i++)
                {
                    ushort glyphIndex;

                    try
                    {
                        var key = text[i];

                        if (!glyphTypeface.CharacterToGlyphMap.TryGetValue(key, out glyphIndex))
                        {
                            glyphIndex = 42;
                        }
                    }
                    catch (Exception)
                    {
                        glyphIndex = 42;
                    }

                    glyphIndexes[i] = glyphIndex;

                    var width = glyphTypeface.AdvanceWidths[glyphIndex] * 1.0;
                    advanceWidths[i] = width;
                }

                try
                {
#pragma warning disable 618
                    var glyphRun = new GlyphRun(glyphTypeface, 0, false, RenderingEmSize, glyphIndexes, new Point(0, 0), advanceWidths, null, null, null, null, null, null);
#pragma warning restore 618
                    return glyphRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in generating Glyphrun");
                }
            }

            return null;
        }

        public static void RegisterFont(string name, FontFamily fontFamily)
        {
            Argument.IsNotNullOrWhitespace(() => name);
            Argument.IsNotNull(() => fontFamily);

            RegisteredFontFamilies[name] = fontFamily;
        }

        public static IEnumerable<string> GetRegisteredFonts()
        {
            return RegisteredFontFamilies.Keys.ToList();
        }

        public static FontFamily GetRegisteredFont(string name)
        {
            Argument.IsNotNullOrWhitespace(() => name);

            return RegisteredFontFamilies.ContainsKey(name) ? RegisteredFontFamilies[name] : null;
        }
    }
}
