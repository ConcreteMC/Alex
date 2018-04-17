using System;
using Alex.API.Graphics;
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
	    private float _scale = 1f;
	    private BitmapFont _font;
	    private SpriteFont _backupFont;
	    private float _rotation;
	    private Vector2? _rotationOrigin;

	    public override Vector2 RotationOrigin
	    {
		    get
		    {
			    return _rotationOrigin.HasValue ? _rotationOrigin.Value : new Vector2(-0.5f,-0.5f);

		    }
		    set { _rotationOrigin = value; }
	    }

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
                    _textShadowOffset = new Vector2(1f, 1f) * (RenderSize.Y * 0.1f);
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

	    public BitmapFont Font
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
		
		private string _renderText = String.Empty;
	    private Vector2 _rotationOrigin1;

	    public GuiTextElement(bool hasBackground = false)
	    {
		    if (hasBackground)
		    {
			    BackgroundOverlayColor = DefaultTextBackgroundColor;
		    }
	    }

		protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            Font = renderer.Font;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
        }

        protected override void OnDraw(GuiRenderArgs renderArgs)
        {
	        var text = _renderText;
            if (!string.IsNullOrWhiteSpace(text))
            {
	            if (Font != null)
	            {
					renderArgs.DrawString(Font, text, RenderPosition, TextColor, HasShadow, Rotation, RotationOrigin, Scale);
	            }
				else if (BackupFont != null)
	            {
		            if (HasShadow)
		            {
			            renderArgs.DrawString(RenderPosition + TextShadowOffset, BackupFont, text, TextColor.BackgroundColor, Scale, Rotation, RotationOrigin);
		            }

		            renderArgs.DrawString(RenderPosition, BackupFont, text, TextColor.ForegroundColor, Scale, Rotation, RotationOrigin);
				}
			}
        }


	    private Vector2 GetSize(string text, Vector2 scale)
	    {
		    return Font?.MeasureString(text, scale) ?? (BackupFont.MeasureString(text));
		}

	    private void OnTextUpdated(bool updateLayout = true)
	    {
		    string text = _text;
		    if (string.IsNullOrWhiteSpace(text))
		    {
			    _renderText = string.Empty;
			    Width = 0;
			    Height = 0;
		    }
		    else if ((Font != null || BackupFont != null))
			{
				var scale = new Vector2(Scale, Scale);

				var textSize = GetSize(text, scale);
				
				Width = (int)Math.Floor(textSize.X);
				Height = (int)Math.Floor(textSize.Y);

				_renderText = text;
	        }

		    if (updateLayout)
		    {
			    ParentElement?.UpdateLayout();
		    }
		}
    }
}
