using System;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
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

        public TextColor TextColor { get; set; } = TextColor.White;

        private Vector2? _textShadowOffset;

        public Vector2 TextShadowOffset
        {
            get
            {
                if (!_textShadowOffset.HasValue)
                {
                    _textShadowOffset = new Vector2(1f, 1f) * (Size.Y * 0.1f);
                    //return new Vector2(1f, -1f) * Scale * 0.125f;
                }
                return _textShadowOffset.Value;
            }

            set => _textShadowOffset = value;
        }

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

        public bool HasShadow { get; set; } = true;

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
                if (HasShadow)
                {
                    renderArgs.DrawString(Position + TextShadowOffset, Font, Text, TextColor.BackgroundColor, Scale);
                }

                renderArgs.DrawString(Position, Font, Text, TextColor.ForegroundColor, Scale);
            }
        }

        private void OnTextUpdated()
        {
            var size = Font?.MeasureString(Text) ?? Vector2.Zero;

            Width  = (int)Math.Ceiling(size.X * Scale);
            Height = (int)Math.Ceiling(size.Y * Scale);

			UpdateLayout();
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
