using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiTextInput : GuiValuedControl<string>
    {
	    private TextColor _textColor        = TextColor.White;
	    private TextColor _inactiveTextColor = TextColor.Gray;

        public Color BorderColor { get; set; } = Color.LightGray;

        public Thickness BorderThickness { get; set; } = Thickness.One;
		
	    public TextColor TextColor
	    {
		    get => _textColor;
		    set
		    {
			    _textColor = value;
				UpdateDisplayText();
		    }
	    }
		public TextColor InactiveTextColor
	    {
		    get => _inactiveTextColor;
		    set 
		    { 
			    _inactiveTextColor = value;
			    UpdateDisplayText();
		    }
	    }

	    private TextInputBuilder _textBuilder;

        private GuiTextElement _textElement;

        private int _cursorPositionX;
        private float _cursorAlpha;

	    public string PlaceHolder { get; set; } = string.Empty;

        public GuiTextInput(string text = null)
        {
            MinWidth = 100;
            MinHeight = 20;
            BackgroundOverlay = Color.Black;

            _textBuilder = new TextInputBuilder(text);
            AddChild(_textElement = new GuiTextElement()
            {
                Anchor = Alignment.MiddleLeft,
				Text = _textBuilder.Text
            });

	        _textBuilder.TextChanged += (s, v) => Value = v;
	        Value = _textBuilder.Text;


			UpdateDisplayText();
        }

		//private bool _isPlaceholder = false;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);
			
	        if (Focused)
            {
	            var preCursor     = _textBuilder.Text.Substring(0, _textBuilder.CursorPosition);
	            var cursorOffsetX = (int)_textElement.Font.MeasureString(preCursor, _textElement.Scale).X;
	            _cursorPositionX = cursorOffsetX;

                var delta = (float)gameTime.TotalGameTime.TotalMilliseconds / 2;
                _cursorAlpha = (float)MathHelpers.SinInterpolation(0.1f, 0.5f, delta)*2;
            }
        }

        protected override void OnKeyInput(char character, Keys key)
        {
            if (Focused)
            {
	            int originalLength = Value?.Length ?? 0;
                if (key == Keys.Back)
                {
                    _textBuilder.RemoveCharacter();
                }
                else if (key == Keys.Left)
                {
                    _textBuilder.CursorPosition--;
                }
                else if (key == Keys.Right)
                {
                    _textBuilder.CursorPosition++;
                }
                else
                {
                    _textBuilder.AppendCharacter(character);
                }
            }
        }

        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            var bounds = RenderBounds;
            bounds.Inflate(1f, 1f);
	        graphics.DrawRectangle(bounds, BorderColor, BorderThickness);

            // Text
            if (Focused)
            {
                if (gameTime.TotalGameTime.Seconds % 2 == 0)
                {
                    var textElementBounds = _textElement.RenderBounds;
                    var offsetX = textElementBounds.X + _cursorPositionX + 1;

					graphics.DrawLine(new Vector2(offsetX, textElementBounds.Top), new Vector2(offsetX, textElementBounds.Bottom), _textColor.ForegroundColor * _cursorAlpha);
	                //graphics.DrawLine(offsetX, textElementBounds.Top, offsetX, textElementBounds.Bottom, _textColor.ForegroundColor * _cursorAlpha);
                }
            }

        }

	    protected override void OnFocusActivate()
	    {
		    UpdateDisplayText();
	    }
	    protected override void OnFocusDeactivate()
	    {
		    UpdateDisplayText();
	    }

	    private void UpdateDisplayText()
	    {
			UpdateDisplayText(Value);
	    }

	    private void UpdateDisplayText(string value)
	    {
		    if (string.IsNullOrEmpty(value))
		    {
			    _textElement.TextColor = InactiveTextColor;

			    if (!string.IsNullOrWhiteSpace(PlaceHolder))
			    {
				    _textElement.Text = PlaceHolder;
			    }
			    else
			    {
				    _textElement.Text = " ";
			    }
		    }
		    else
		    {
			    _textElement.TextColor = Focused ? TextColor : InactiveTextColor;
			    _textElement.Text      = value;
		    }
	    }
		
	    protected override bool OnValueChanged(string value)
	    {
		    if (!_textBuilder.Text.Equals(value))
		    {
			    _textBuilder.Text = value;
		    }

		    UpdateDisplayText(value);
		    return true;
	    }
    }
}
