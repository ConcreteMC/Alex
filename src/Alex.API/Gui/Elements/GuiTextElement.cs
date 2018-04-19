using System;
using Alex.API.Graphics;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BitmapFont = Alex.API.Graphics.Typography.BitmapFont;

namespace Alex.API.Gui.Elements
{
    public class GuiTextElement : GuiElement
    {
	    public static readonly Color DefaultTextBackgroundColor = new Color(Color.Black, 0.6f);
        
	    private string _text;
	    private Vector2? _textShadowOffset;
	    private float _opacity = 1f;
	    private Vector2 _scale = Vector2.One;
	    private IFont _font;
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
                _text = value ?? string.Empty;
                OnTextUpdated();
            }
        }

        public TextColor TextColor { get; set; } = TextColor.White;
		
	    public float Opacity
	    {
		    get => _opacity;
		    set => _opacity = value;
	    }

	    public float Scale
        {
            get => _scale.X;
            set
            {
                _scale = new Vector2(value);
                OnTextUpdated();
            }
        }

	    private FontStyle _fontStyle;

	    public FontStyle FontStyle
	    {
		    get => _fontStyle;
		    set => _fontStyle = value;
	    }

	    public bool HasShadow { get; set; } = true;

	    public IFont Font
	    {
		    get => _font;
		    set
		    {
			    _font = value;
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

			Margin = new Thickness(5, 5);
	    }

		protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);

            Font = renderer.Font;
        }


        protected override void OnDraw(GuiRenderArgs renderArgs)
        {
	        var text = _renderText;
            if (!string.IsNullOrWhiteSpace(text))
            {
	            if (Font != null)
	            {
					Font.DrawString(renderArgs.SpriteBatch, text, RenderPosition, TextColor, FontStyle, scale: _scale, rotation: Rotation, origin: RotationOrigin, opacity: Opacity);
	            }
			}
        }


	    private Vector2 GetSize(string text, Vector2 scale)
	    {
		    return Font?.MeasureString(text, scale) ?? Vector2.Zero;
		}

	    private void OnTextUpdated()
	    {
		    string text = _text;
			if (Font != null && !string.IsNullOrWhiteSpace(text))
		    if (string.IsNullOrWhiteSpace(text))
		    {
			    _renderText = string.Empty;
			    Width = 0;
			    Height = 0;
			    
			    InvalidateLayout();
		    }
		    else if (Font != null)
			{
				var scale = new Vector2(Scale, Scale);

				var textSize = GetSize(text, scale);
				
				Width = (int)Math.Floor(textSize.X);
				Height = (int)Math.Floor(textSize.Y);

				_renderText = text;

				InvalidateLayout();
	        }
		}
    }
}
