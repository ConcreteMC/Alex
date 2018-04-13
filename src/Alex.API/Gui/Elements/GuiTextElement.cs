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

        private IFontRenderer _font;
        public IFontRenderer Font
        {
            get => _font;
            set
            {
                _font = value;
                OnTextUpdated();
            }
        }

	    private SpriteFont _backupFont;
		public SpriteFont BackupFont
	    {
		    get => _backupFont;
		    set
		    {
			    _backupFont = value;
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
	        var text = Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
	            if (Font != null)
	            {
		            if (HasShadow)
		            {
			            renderArgs.DrawString(Font, text, Position + TextShadowOffset, TextColor.BackgroundColor, Scale);
		            }

		            renderArgs.DrawString(Font, text, Position, TextColor.ForegroundColor, Scale);
	            }
				else if (BackupFont != null)
	            {
		            if (HasShadow)
		            {
			            renderArgs.DrawString(Position + TextShadowOffset, BackupFont, text, TextColor.BackgroundColor, Scale);
		            }

		            renderArgs.DrawString(Position, BackupFont, text, TextColor.ForegroundColor, Scale);
				}
			}
        }

	    private void OnTextUpdated(bool updateLayout = true)
        {
	        if ((Font != null || BackupFont != null) && !string.IsNullOrWhiteSpace(Text))
	        {
		        var size = Font?.GetStringSize(Text, new Vector2(Scale)) ?? BackupFont.MeasureString(Text);

		        Width = (int) Math.Ceiling(size.X);
		        Height = (int) Math.Ceiling(size.Y);

		        if (updateLayout)
		        {
					ParentElement.UpdateLayout();
		        }
	        }
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
