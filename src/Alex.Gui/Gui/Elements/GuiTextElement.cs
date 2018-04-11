using System;
using Alex.Graphics.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui.Elements
{
    public class GuiTextElement : GuiElement
    {
        private string _text;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnTextUpdated();
            }
        }

        private SpriteFont _font;

        public SpriteFont Font
        {
            get => _font;
            set
            {
                _font = value;
                OnTextUpdated();
            }
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            Font = renderer.DefaultFont;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
        }

        protected override void OnDraw(GuiRenderArgs renderArgs)
        {
            
        }

        private void OnTextUpdated()
        {
            var size = Font?.MeasureString(Text) ?? Vector2.Zero;

            Width  = (int)Math.Ceiling(size.X);
            Height = (int)Math.Ceiling(size.Y);
        }
    }
}
