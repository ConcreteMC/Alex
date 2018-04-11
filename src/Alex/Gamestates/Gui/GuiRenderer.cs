using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Alex.Graphics.Gui.Rendering;
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
                    return  TextureUtils.BitmapToTexture2D(_graphicsDevice, _widgets.Clone(WidgetHotBar, PixelFormat.Format32bppPArgb));

                case GuiTextures.Inventory_HotBar_SelectedItemOverlay:
                    return  TextureUtils.BitmapToTexture2D(_graphicsDevice, _widgets.Clone(WidgetHotBarSelectedOverlay, PixelFormat.Format32bppPArgb));
            }

            return new Texture2D(_graphicsDevice, 1, 1);
        }
    }
}
