using System;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui.Elements
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

        public Color TextColor { get; set; } = Color.Black;


        private float _scale = 0.5f;

        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
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
            base.OnInit(renderer);
            Font = renderer.DefaultFont;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
        }

        protected override void OnDraw(GuiRenderArgs renderArgs)
        {
            if (!string.IsNullOrWhiteSpace(Text) && Font != null)
            {
                renderArgs.DrawString(Position, Font, Text, TextColor, Scale);
            }
        }

        private void OnTextUpdated()
        {
            var size = Font?.MeasureString(Text) ?? Vector2.Zero;

            size *= Scale;

            Width  = (int)Math.Ceiling(size.X);
            Height = (int)Math.Ceiling(size.Y);
        }
    }

    public class GuiAutoUpdatingTextElement : GuiTextElement
    {
        private readonly Func<string> _updateFunc;

        public GuiAutoUpdatingTextElement(Func<string> updateFunc) : base()
        {
            _updateFunc = updateFunc;
            Text = _updateFunc();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            Text = _updateFunc();
        }
    }
}
