using System;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui.Elements
{
    public class GuiTextElement : GuiElement
    {
	    public static readonly Color DefaultTextBackgroundColor = new Color(Color.Black, 0.6f);
        
	    private string _text;
	    private Vector2? _textShadowOffset;
	    private float _scale = 0.5f;
	    private IFontRenderer _font;
	    private SpriteFont _backupFont;
	    private float _rotation;
	    private Vector2 _rotationOrigin;


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

        public IFontRenderer Font
        {
            get => _font;
            set
            {
                _font = value;
                OnTextUpdated();
            }
        }

		public SpriteFont BackupFont
	    {
		    get => _backupFont;
		    set
		    {
			    _backupFont = value;
				OnTextUpdated();
		    }
	    }
		
	    public GuiTextElement(bool hasBackground = false)
	    {
			RotationOrigin = new Vector2(0.5f);
		    if (hasBackground)
		    {
			    BackgroundOverlayColor = DefaultTextBackgroundColor;
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
			            renderArgs.DrawString(Font, text, Position + TextShadowOffset, TextColor.BackgroundColor, Scale, Rotation, RotationOrigin);
		            }

		            renderArgs.DrawString(Font, text, Position, TextColor.ForegroundColor, Scale, Rotation, RotationOrigin);
	            }
				else if (BackupFont != null)
	            {
		            if (HasShadow)
		            {
			            renderArgs.DrawString(Position + TextShadowOffset, BackupFont, text, TextColor.BackgroundColor, Scale, Rotation, RotationOrigin);
		            }

		            renderArgs.DrawString(Position, BackupFont, text, TextColor.ForegroundColor, Scale, Rotation, RotationOrigin);
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
}
