using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Alex.Gui;
using Alex.Gui.Controls;
using Alex.Gui.Controls.Menu;
using Alex.Gui.Layout;
using Alex.Gui.Rendering;
using Alex.Gui.Themes;
using Alex.ResourcePackLib;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Alex.Rendering
{
    public class ResourcePackUiThemeBuilder
    {

        private GraphicsDevice Graphics { get;}
        private McResourcePack ResourcePack { get; set; }
        private UiTheme Theme { get;}
        private bool ReplaceExisting { get; set; }
        private Dictionary<string, NinePatchTexture> Textures { get; }

        public ResourcePackUiThemeBuilder(GraphicsDevice graphicsDevice)
        {
            Graphics = graphicsDevice;
            Theme = new UiTheme();
            Textures = new Dictionary<string, NinePatchTexture>();
        }

        public void LoadResources(ResourceManager resourceManager, McResourcePack resourcePack, bool replaceExisting)
        {
            ResourcePack = resourcePack;
            ReplaceExisting = replaceExisting;
            Build();
        }

        public UiTheme GetTheme()
        {
            return Theme;
        }

        private void Build()
        {
            BuildPanelStyles();
            BuildButtonStyles();
        }

        private void BuildPanelStyles()
        {
            Theme.AddClass("TitleScreenRoot", new UiElementStyle()
            {
                Background = (NinePatchTexture) TextureUtils.ImageToTexture2D(Graphics, Resources.mcbg)
            });
            Theme.AddClass("TitleScreenLogo", new UiElementStyle()
            {
                Background = (NinePatchTexture)TextureUtils.ImageToTexture2D(Graphics, Resources.logo)
            });

            Theme.AddClass<UiPanel>(i => i.ClassName == null, new UiElementStyle()
            {
                BackgroundColor = new Color(Color.Black, 0.2f)
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
            Theme.AddClass<UiMenuItem>(i => i.IsMouseDown && i.IsMouseOver, new UiElementStyle()
            {
                Background = GetTexture("classic-button-pressed"),
            });
        }

        private NinePatchTexture GetTexture(string fileName)
        {
            NinePatchTexture texture;
            if (!Textures.TryGetValue(fileName, out texture) || ReplaceExisting)
            {
                if (TryLoadTexture2D(fileName + ".png", out var rawTexture))
                {
                    var ninePatch = GetNinePatchSize(fileName);
                
                    texture = new NinePatchTexture(rawTexture, ninePatch);
                    Textures.Add(fileName, texture);
                }
            }

            return texture;
        }

        private bool TryLoadTexture2D(string fileName, out Texture2D texture)
        {
            var path = Path.Combine("textures", "ui", fileName);

            if (ResourcePack.TryGetStream(path, out var stream))
            {
                texture = Texture2D.FromStream(Graphics, stream);
                return true;
            }

            texture = new Texture2D(Graphics, 2, 2, false, SurfaceFormat.Color);
            texture.SetData(new []
            {
                Color.Black,
                Color.Purple,
                Color.Purple,
                Color.Black
            });
            
            return false;
        }

        private int GetNinePatchSize(string fileName)
        {
            if (ResourcePack.TryGetJson($"textures\\ui\\{fileName}", out NinePatchTextureConfig json))
            {
                return json.NineSliceSize;
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
