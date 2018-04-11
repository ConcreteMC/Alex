using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.Textures;
using Alex.Graphics.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui.Elements
{
    public class GuiProgressBar : GuiElement
    {

        public int MinValue { get; set; } = 0;
        public int Value { get; set; } = 0;
        public int MaxValue { get; set; } = 100;

        public float Percent => Math.Max(0, Math.Min(1, Value / (float)Math.Abs(MaxValue - MinValue)));

        private int _spriteSheetSegmentWidth = 1;
        public NinePatchTexture Highlight { get; set; }

        public GuiProgressBar()
        {
            DebugColor = Color.LavenderBlush;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            var texture = renderer.GetTexture(GuiTextures.ProgressBar);
            var b = texture.Bounds;

            _spriteSheetSegmentWidth = (int)Math.Round(b.Width / 4f);
            Background = new NinePatchTexture(texture, new Rectangle(b.X, b.Y, _spriteSheetSegmentWidth * 3, b.Height), _spriteSheetSegmentWidth);
            Highlight = new NinePatchTexture(texture, new Rectangle(_spriteSheetSegmentWidth * 3, b.Y, _spriteSheetSegmentWidth, b.Height), _spriteSheetSegmentWidth);
        }

        protected override void OnDraw(GuiRenderArgs args)
        {
            var bounds = Bounds;

            var fillWidth = bounds.Width - 2 * _spriteSheetSegmentWidth;

            base.OnDraw(args);

            bounds = new Rectangle(bounds.X + _spriteSheetSegmentWidth, bounds.Y, (int)(fillWidth * Percent), bounds.Height);
            args.DrawNinePatch(bounds, Highlight, TextureRepeatMode.Stretch);
        }
    }
}
