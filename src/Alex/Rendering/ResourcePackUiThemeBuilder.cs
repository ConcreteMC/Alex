using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Alex.Graphics;
using Alex.Graphics.Textures;
using Alex.Graphics.UI;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Controls.Menu;
using Alex.Graphics.UI.Layout;
using Alex.Graphics.UI.Themes;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Alex.Rendering
{
    public class ResourcePackUiThemeBuilder
    {

        private GraphicsDevice Graphics { get; }
        private BedrockResourcePack ResourcePack { get; set; }
        private UiTheme Theme { get; }
        private Dictionary<string, NinePatchTexture> Textures { get; }

        public ResourcePackUiThemeBuilder(GraphicsDevice graphicsDevice)
        {
            Graphics = graphicsDevice;
            Theme = new UiTheme();
            Textures = new Dictionary<string, NinePatchTexture>();
        }

        public void LoadResources(BedrockResourcePack resourcePack)
        {
            ResourcePack = resourcePack;
            Build();
        }

        public UiTheme GetTheme()
        {
            return Theme;
        }

        private void Build()
        {
            BuildBaseStyles();
            BuildPanelStyles();
            BuildButtonStyles();
        }

        private void BuildBaseStyles()
        {
            Theme.AddClass<UiRoot>(new UiElementStyle()
            {
                PositionAnchor = Vector2.Zero,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                SizeAnchor = Vector2.One
            });
            Theme.AddClass("GuiRoot", new UiElementStyle()
            {
                SizeAnchor = Vector2.One
            });

            Theme.AddClass<UiElement>(new UiElementStyle()
            {
                TextFont = Alex.Font,
                TextColor = Color.Black,
            });
        }

        private void BuildPanelStyles()
        {
            Theme.AddClass("TitleScreenRoot", new UiElementStyle()
            {
                StylePriority = 100,
                Background = GetTexture("background"),
                BackgroundRepeat = TextureRepeatMode.Tile,
                SizeAnchor = Vector2.One
            });

            Theme.AddClass("TitleScreenLogo", new UiElementStyle()
            {
                Background = (NinePatchTexture)TextureUtils.ImageToTexture2D(Graphics, Resources.logo),
                PositionAnchor = new Vector2(0.5f,0.1f),
                Width = 250,
                Height = 100
            });

            Theme.AddClass<UiPanel>(i => string.IsNullOrWhiteSpace(i.ClassName), new UiElementStyle()
            {
                BackgroundColor = new Color(Color.Black, 0.2f),
                SizeAnchor = new Vector2(0.25f, 1.0f),
                PositionAnchor = new Vector2(0.15f, 0.0f)
            });
        }


        private void BuildButtonStyles()
        {
            Theme.AddClass<UiMenuItem>(i => !i.IsMouseDown && !i.IsMouseOver, new UiElementStyle()
            {
                Background = GetTexture("classic-button"),
            });
            Theme.AddClass<UiMenuItem>(i => !i.IsMouseDown && i.IsMouseOver, new UiElementStyle()
            {
                Background = GetTexture("classic-button-hover"),
            });
            Theme.AddClass<UiMenuItem>(i => i.IsMouseDown, new UiElementStyle()
            {
                Background = GetTexture("classic-button-pressed"),
            });
            Theme.AddClass<UiMenuItem>(new UiElementStyle()
            {
                Width     = 200,
                TextFont  = Alex.Font,
                TextColor = Color.Black,
                TextSize  = 0.5f,
                Padding   = new Thickness(10),
                Margin    = new Thickness(5)
            });
        }

        private NinePatchTexture GetTexture(string fileName)
        {
            NinePatchTexture texture;
            if (!Textures.TryGetValue(fileName, out texture))
            {
                var rawTexture = LoadTexture2D(fileName);
                var ninePatch = GetNinePatchSize(fileName);

                texture = new NinePatchTexture(rawTexture, ninePatch);
                Textures.Add(fileName, texture);
            }

            return texture;
        }

        private Texture2D LoadTexture2D(string fileName)
        {
            if (ResourcePack.TryGetTexture($"textures/ui/{fileName}", out var bitmap))
            {
                return TextureUtils.BitmapToTexture2D(Graphics, bitmap);
            }

            var texture = new Texture2D(Graphics, 2, 2, false, SurfaceFormat.Color);
            texture.SetData(new[]
            {
                Color.Black,
                Color.Purple,
                Color.Purple,
                Color.Black
            });

            return texture;
        }

        private int GetNinePatchSize(string fileName)
        {
            if (ResourcePack.TryGetTextureJson($"textures/ui/{fileName}", out var json))
            {
                var obj = MCJsonConvert.DeserializeObject<NinePatchTextureConfig>(json);
                return obj.NineSliceSize;
            }

            return 0;
        }

    }

    public class NinePatchTextureConfig
    {
        [JsonProperty("nineslice_size")]
        public int NineSliceSize { get; set; }

        [JsonProperty("base_size")]
        public int[] BaseSize { get; set; }
    }
}
