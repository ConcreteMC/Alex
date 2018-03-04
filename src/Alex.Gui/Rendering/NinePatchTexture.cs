using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Rendering
{
    public class NinePatchTexture
    {
        public int NineSliceSize { get; }

        public Texture2D Texture { get; }

        public NinePatchTexture(Texture2D texture, int sizeSlice = 0)
        {
            Texture = texture;
            NineSliceSize = sizeSlice;
        }

        public static explicit operator NinePatchTexture(Texture2D texture)
        {
            return new NinePatchTexture(texture);
        }
    }
}
