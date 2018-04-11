using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.Gui.Rendering;
using Alex.Utils;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates.Gui
{
    public class GuiRenderer : IGuiRenderer
    {
        public void Init(GraphicsDevice graphics)
        {
            _graphicsDevice = graphics;
        }

        public SpriteFont DefaultFont => Alex.Font;

        private GraphicsDevice _graphicsDevice;
        public GuiRenderer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public Texture2D GetTexture(GuiTextures guiTexture)
        {
            switch (guiTexture)
            {
                case GuiTextures.Inventory_HotBar:
                    return TextureUtils.ImageToTexture2D(_graphicsDevice, Resources.ItemHotbar);

                case GuiTextures.Inventory_HotBar_SelectedItemOverlay:
                    return TextureUtils.ImageToTexture2D(_graphicsDevice, Resources.ItemHotbar_SelectedItem);
            }

            return new Texture2D(_graphicsDevice, 1, 1);
        }
    }
}
