using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Alex.API.Graphics.Typography;

using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;

using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements
{
	public class GuiTextClickedEvent : EventArgs
	{
		public Uri ClickedText;
	}

    public class GuiTextElement : GuiElement //GuiControl
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

		public TextAlignment TextAlignment { get; set; } = TextAlignment.None;

		private bool _fixedSize = false;
		[DebuggerVisible]
		public bool HasFixedSize
		{
			get
			{
				return _fixedSize;
			}
			set
			{
				_fixedSize = value;
			}
		}
		
		private string _renderText = String.Empty;

		public EventHandler<GuiTextClickedEvent> OnLinkClicked;

		//public void AddClickable()

		/*public class ClickableElement
		{
			public Rectangle Area { get; set; } 
			public Action<GuiTextElement, string> ClickAction { get; set; }
			public string Text { get; set; }
		}*/

		//private List<ClickableElement> ClickableElements = new List<ClickableElement>();

		private bool _wrap = false;

		public bool Wrap
		{
			get => _wrap;
			set
			{
				_wrap = value;
				OnTextUpdated();
			}
		}
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
		        //if (HasBackground && BackgroundOverlay.HasValue && BackgroundOverlay.Color.HasValue)
		        // {
		        //   graphics.SpriteBatch.FillRectangle(new Rectangle(RenderPosition.ToPoint(), Size /*GetSize(text, _scale).ToPoint()*/), BackgroundOverlay.Color.Value);
		        // }
		        var renderPosition = RenderPosition;

		        foreach (var line in text.Split('\n'))
		        {
			        var size     = Font.MeasureString(line, _scale);
			        var position = renderPosition;

			        if ((TextAlignment & TextAlignment.Right) != 0)
			        {
				        position.X = RenderBounds.Right - size.X;
			        }
			        
			        if ((TextAlignment & TextAlignment.Center) != 0)
			        {
				        position.X = RenderBounds.Left + (size.X / 2f);
			        }

			        if (HasBackground && BackgroundOverlay.HasValue && BackgroundOverlay.Color.HasValue)
			        {
				        var p = position.ToPoint();
				        var s = size.ToPoint();

				        graphics.SpriteBatch.FillRectangle(
					        new Rectangle(p, s),
					        BackgroundOverlay.Color.Value);
			        }

			        Font.DrawString(
				        graphics.SpriteBatch, line, position, TextColor, FontStyle, _scale, TextOpacity, Rotation,
				        RotationOrigin);

			        renderPosition.Y += size.Y;
		        }
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
			
			string text = _renderText;
			
			var textSize = GetSize(text, scale);
			
			/*if (_wrap && textSize.X > MaxWidth)
			{
				StringBuilder sb    = new StringBuilder();
				var           split = text.Split(' ');

				int startIndex = 0;
				int length     = split.Length - 1;
				var sizeX      = textSize.X;

				string line = "";
				while (sizeX > MaxWidth)
				{
					length--;
					
					line = string.Join(' ', split, startIndex, length);
					var ts   = GetSize(line, scale);
					
					sizeX = ts.X;
				}

				sb.AppendLine(line);
				var lineLength = line.Length;

				while (true)
				{
					string l = "";
				}
				
				//var middle = string.Join(' ', split, 0, split.Length / 2);
				//textSize = GetSize(middle, scale);

				for (int i = 0; i < split.Length; i++)
				{
					
				}
			}*/

			size = new Size((int)Math.Ceiling(textSize.X), (int)Math.Ceiling(textSize.Y));
			minSize = size;
			maxSize = size;
		}

		//private static Regex LinkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		protected virtual void OnTextUpdated()
	    {
		    string text = _text;
			//if (Font != null && !string.IsNullOrWhiteSpace(text))
		    if (string.IsNullOrWhiteSpace(text))
		    {
			    _renderText = string.Empty;
			    Width = 0;
			    Height = 0;

			    if (!HasFixedSize)
			    {
				   // InvalidateLayout();
			    }
			    else
			    {
				    
			    }
		    }
		    else
			{
				//var scale = new Vector2(Scale, Scale);
				
				_renderText = text;

				GetPreferredSize(out var size, out var minSize, out var maxSize);
				Width = Math.Max(Math.Min(size.Width, maxSize.Width), minSize.Width);// size.Width;
				Height = Math.Max(Math.Min(size.Height, maxSize.Height), minSize.Height);// size.Height;
			}
		}

		//protected override void OnCursorPressed(Point cursorPosition, MouseButton button)
	//	{
		//	base.OnCursorPressed(cursorPosition, button);
		/*	foreach (var c in ClickableElements.ToArray())
			{
				if (c.Area.Contains(cursorPosition))
				{
					OnLinkClicked?.Invoke(this, new GuiTextClickedEvent() { ClickedText = new Uri(c.Text) });
					//c.ClickAction?.Invoke(this, c.Text);
				}
			}*/
		//}
	}
}
