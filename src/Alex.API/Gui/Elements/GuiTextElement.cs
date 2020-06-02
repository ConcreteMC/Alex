using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Alex.API.Graphics.Typography;

using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;

using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.API.Gui.Elements
{
	public class GuiTextClickedEvent : EventArgs
	{
		public Uri ClickedText;
	}

    public class GuiTextElement : GuiControl
	{
	    public static readonly Color DefaultTextBackgroundColor = new Color(Color.Black, 0.6f);
        
	    private string _text;
	    private float _textOpacity = 1f;
	    private Vector2 _scale = Vector2.One;
	    private Vector2? _rotationOrigin;
	    private IFont _font;
		
		[DebuggerVisible] public override Vector2 RotationOrigin
	    {
		    get
		    {
			    return _rotationOrigin.HasValue ? _rotationOrigin.Value : new Vector2(-0.5f,-0.5f);

		    }
		    set { _rotationOrigin = value; }
	    }

	    private string _translationKey;

		[DebuggerVisible] public string TranslationKey
	    {
		    get => _translationKey;
		    set 
		    { 
			    _translationKey = value;
			    OnTranslationKeyUpdated();
		    }
	    }
		
		[DebuggerVisible] public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                OnTextUpdated();
            }
        }
		[DebuggerVisible] public TextColor TextColor { get; set; } = TextColor.White;
		[DebuggerVisible] public float TextOpacity
	    {
		    get => _textOpacity;
		    set => _textOpacity = value;
	    }

		[DebuggerVisible] public float Scale
        {
            get => _scale.X;
            set
            {
                _scale = new Vector2(value);
                OnTextUpdated();
            }
        }

	    private FontStyle _fontStyle;

		[DebuggerVisible] public FontStyle FontStyle
	    {
		    get => _fontStyle;
		    set => _fontStyle = value;
	    }

	    public bool HasShadow { get; set; } = true;

		[DebuggerVisible] public IFont Font
	    {
		    get => _font;
		    set
		    {
			    _font = value;
			    OnTextUpdated();
		    }
	    }

		private string _renderText = String.Empty;

		public EventHandler<GuiTextClickedEvent> OnLinkClicked;

		//public void AddClickable()

		public class ClickableElement
		{
			public Rectangle Area { get; set; } 
			public Action<GuiTextElement, string> ClickAction { get; set; }
			public string Text { get; set; }
		}

		private List<ClickableElement> ClickableElements = new List<ClickableElement>();

		public GuiTextElement(string text, bool hasBackground = false) : this(hasBackground)
		{
			Text = text;
		}
		
		private bool HasBackground { get; }
	    public GuiTextElement(bool hasBackground = false)
	    {
		    HasBackground = hasBackground;
		    if (hasBackground)
		    {
			    BackgroundOverlay = DefaultTextBackgroundColor;
		    }
		    else
		    {
			    Background = null;
			    BackgroundOverlay = null;
		    }

			Margin = new Thickness(2);
		}

		protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);

            Font = renderer.Font;

	        OnTranslationKeyUpdated();
        }

		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
	        var text = _renderText;
            if (!string.IsNullOrWhiteSpace(text))
            {
	            //base.OnDraw(graphics, gameTime);
	            
				/*var size = Font.MeasureString(text, Scale);
				while (size.X > RenderBounds.Width && text.Length >= 1)
				{
					text = text.Substring(0, text.Length - 1);
					size = Font.MeasureString(text, Scale);
				}*/
				
	           // graphics.FillRectangle(new Rectangle(RenderPosition.ToPoint(), Size.ToPoint()), BackgroundOverlay);
	           if (HasBackground && BackgroundOverlay.HasValue && BackgroundOverlay.Color.HasValue)
	           {
		           graphics.SpriteBatch.FillRectangle(new Rectangle(RenderPosition.ToPoint(), GetSize(text, _scale).ToPoint()), BackgroundOverlay.Color.Value);
	           }

	           Font.DrawString(graphics.SpriteBatch, text, RenderPosition, TextColor, FontStyle, _scale, TextOpacity, Rotation, RotationOrigin);
	          //  graphics.DrawString(RenderPosition, text, Font, TextColor, FontStyle, Scale, Rotation, RotationOrigin, TextOpacity);
			}
        }


	    private Vector2 GetSize(string text, Vector2 scale)
	    {
		    var size= Font?.MeasureString(text, scale) ?? Vector2.Zero;
            if (FontStyle.HasFlag(FontStyle.Bold))
            {
                size.X += text.Length * scale.X;
            }

            return size;
        }

	    private void OnTranslationKeyUpdated()
	    {
		    if (!string.IsNullOrEmpty(TranslationKey))
		    {
			    Text = GuiRenderer?.GetTranslation(TranslationKey);
		    }
	    }

		protected override void GetPreferredSize(out Size size, out Size minSize, out Size maxSize)
		{
			base.GetPreferredSize(out size, out minSize, out maxSize);
			var scale = new Vector2(Scale, Scale);

			string text = _text;
			var textSize = GetSize(text, scale);

			size = new Size((int)Math.Ceiling(textSize.X), (int)Math.Ceiling(textSize.Y));
		}

		private static Regex LinkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		protected virtual void OnTextUpdated()
	    {
		    string text = _text;
			//if (Font != null && !string.IsNullOrWhiteSpace(text))
		    if (string.IsNullOrWhiteSpace(text))
		    {
			    _renderText = string.Empty;
			    Width = 0;
			    Height = 0;
			    
			    InvalidateLayout();
		    }
		    else
			{
				var scale = new Vector2(Scale, Scale);

				//PreferredSize = new Size((int)Math.Floor(textSize.X), (int)Math.Floor(textSize.Y));
				//Width = (int)Math.Floor(textSize.X);
				//Height = (int)Math.Floor(textSize.Y);

				_renderText = text;

				InvalidateLayout();

				foreach (Match match in LinkParser.Matches(text))
				{
					var l = GetSize(text.Substring(0, match.Index), scale);
					var linkSize = GetSize(match.Value, scale);

					Rectangle clickArea = new Rectangle((int)l.X, 0, (int)linkSize.X, (int)linkSize.Y);

					ClickableElements.Add(new ClickableElement()
					{
						Area = clickArea,
						//ClickAction = (s, val) => {
							
						//},
						Text = match.Value
					});
				}
			}
		}

		protected override void OnCursorPressed(Point cursorPosition)
		{
			base.OnCursorPressed(cursorPosition);
			foreach (var c in ClickableElements.ToArray())
			{
				if (c.Area.Contains(cursorPosition))
				{
					OnLinkClicked?.Invoke(this, new GuiTextClickedEvent() { ClickedText = new Uri(c.Text) });
					//c.ClickAction?.Invoke(this, c.Text);
				}
			}
		}
	}
}
