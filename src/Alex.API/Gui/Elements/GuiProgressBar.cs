using System;
using Alex.API.Graphics;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements
{
    public class GuiProgressBar : GuiElement
    {

        public int MinValue { get; set; } = 0;
        public int Value { get; set; } = 0;
        public int MaxValue { get; set; } = 100;

        public float Percent => Math.Max(0, Math.Min(1, Value / (float)Math.Abs(MaxValue - MinValue)));

        private int _spriteSheetSegmentWidth = 1;
        public NinePatchTexture2D Highlight { get; set; }

        public GuiProgressBar()
        {
            DebugColor = Color.LavenderBlush;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            var texture = renderer.GetTexture(GuiTextures.ProgressBar);
            var b = texture.Bounds;

            _spriteSheetSegmentWidth = (int)Math.Round(b.Width / 4f);
            Background = new NinePatchTexture2D(texture.Texture, new Rectangle(b.X, b.Y, _spriteSheetSegmentWidth * 3, b.Height), _spriteSheetSegmentWidth);
            Highlight = new NinePatchTexture2D(texture.Texture, new Rectangle(_spriteSheetSegmentWidth * 3, b.Y, _spriteSheetSegmentWidth, b.Height), _spriteSheetSegmentWidth);
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
