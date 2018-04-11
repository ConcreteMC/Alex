using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.Textures;
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

        private Bitmap _widgets;

        private static readonly Rectangle WidgetHotBar = new Rectangle(0, 0, 182, 22);
        private static readonly Rectangle WidgetHotBarSelectedOverlay = new Rectangle(0, 22, 24, 24);
        private static readonly Rectangle WidgetCrosshair = new Rectangle(240, 0, 15, 15);

        public GuiRenderer(Alex alex)
        {
            Alex = alex;
            Init(alex.GraphicsDevice);
        }
        public void Init(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;
            _resourceManager = Alex.Resources;
            if (_resourceManager != null)
            {
                _resourceManager.ResourcePack.TryGetTexture("gui/widgets", out _widgets);
            }
        }

        public Texture2D GetTexture(GuiTextures guiTexture)
        {
            switch (guiTexture)
            {
                case GuiTextures.Inventory_HotBar:
                    return _widgets.Slice(_graphicsDevice, WidgetHotBar);

                case GuiTextures.Inventory_HotBar_SelectedItemOverlay:
                    return _widgets.Slice(_graphicsDevice, WidgetHotBarSelectedOverlay);

                case GuiTextures.Crosshair:
                    return _widgets.Slice(_graphicsDevice, WidgetCrosshair);

                case GuiTextures.ProgressBar:
                    return TextureUtils.ImageToTexture2D(_graphicsDevice, Resources.ProgressBar);

                case GuiTextures.SplashBackground:
                    return TextureUtils.ImageToTexture2D(Alex.GraphicsDevice, Resources.Splash);
            }

            return new Texture2D(_graphicsDevice, 1, 1);
        }

    }
}
