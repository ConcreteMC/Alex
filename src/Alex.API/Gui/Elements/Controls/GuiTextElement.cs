using System;
using Microsoft.Xna.Framework;
using RocketUI;
using RocketUI.Elements;
using RocketUI.Graphics;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiTextElement : VisualElement
    {
	    public static readonly Color DefaultTextBackgroundColor = new Color(Color.Black, 0.6f);
        
	    private string _text;
	    private string _renderText = String.Empty;
	    private float _textOpacity = 1f;
	    private Vector2 _scale = Vector2.One;
	    private Vector2? _rotationOrigin;
	    private IFont _font;
	    private FontStyle _fontStyle;

	    public override Vector2 RotationOrigin
	    {
		    get
		    {
			    return _rotationOrigin.HasValue ? _rotationOrigin.Value : new Vector2(-0.5f,-0.5f);

		    }
		    set { _rotationOrigin = value; }
	    }

	    private string _translationKey;

	    public string TranslationKey
	    {
		    get => _translationKey;
		    set 
		    { 
			    _translationKey = value;
			    OnTranslationKeyUpdated();
		    }
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
		public Color Foreground { get; set; } = Color.White;
	    public Color ForegroundShadow { get; set; } = Color.Transparent;
		public float TextOpacity
	    {
		    get => _textOpacity;
		    set => _textOpacity = value;
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
		public FontStyle FontStyle
	    {
		    get => _fontStyle;
		    set => _fontStyle = value;
	    }

	    public IFont Font
	    {
		    get => _font;
		    set
		    {
			    _font = value;
			    OnTextUpdated();
		    }
	    }
		public bool UseDebugFont { get; set; } = false;



	    public GuiTextElement(bool hasBackground = false)
	    {
		    if (hasBackground)
		    {
			    BackgroundOverlay = DefaultTextBackgroundColor;
		    }

			Margin = new Thickness(2);
	    }

		protected override void OnInit()
        {
            base.OnInit();

	        if (Font == null)
	        {
		        Font = UseDebugFont ? renderer.DebugFont : renderer.Font;
	        }

	        OnTranslationKeyUpdated();
        }
		
        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
	        var text = _renderText;
            if (!string.IsNullOrWhiteSpace(text))
            {
				graphics.DrawString(RenderPosition, text, Font, Foreground, ForegroundShadow, FontStyle, Scale, Rotation, RotationOrigin, TextOpacity);
			}
        }
		
	    private Vector2 GetSize(string text, Vector2 scale)
	    {
		    return (Font?.MeasureString(text) * scale) ?? Vector2.Zero;
		}

	    private void OnTranslationKeyUpdated()
	    {
		    if (!string.IsNullOrEmpty(TranslationKey))
		    {
			    Text = GuiRenderer?.GetTranslation(TranslationKey);
		    }
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
