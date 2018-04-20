using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Rendering;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiTextInput : GuiValuedControl<string>
    {
        public Color BorderColor { get; set; } = Color.LightGray;

        public Thickness BorderThickness { get; set; } = Thickness.One;

        private TextInputBuilder _textBuilder;

        private GuiTextElement _textElement;

        private int _cursorPositionX;
        private float _cursorAlpha;

	    public string PlaceHolder { get; set; } = string.Empty;

        public GuiTextInput()
        {
            MinWidth = 100;
            MinHeight = 20;
            BackgroundOverlayColor = Color.Black;

            _textBuilder = new TextInputBuilder();

            AddChild(_textElement = new GuiTextElement()
            {
                Anchor = Alignment.MiddleLeft
            });

	        _textColor = _textElement.TextColor;
	        Value = string.Empty;
        }

	    private TextColor _textColor;
		private bool _isPlaceholder = false;
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            var text = _textBuilder.Text;
	        var l = text.Length;
			if (IsFocused || l > 0)
	        {
		        _textElement.Text = text;
		        _textElement.TextColor = _textColor;
		        _isPlaceholder = false;
	        }
	        else if (!IsFocused && !string.IsNullOrWhiteSpace(PlaceHolder) && l == 0)
			{
				if (!_isPlaceholder)
				{
					_isPlaceholder = true;
					_textColor = _textElement.TextColor;

					_textElement.Text = PlaceHolder;
					_textElement.TextColor = TextColor.Gray;
				}
			}

	        if (IsFocused)
            {
                var preCursor = text.Substring(0, _textBuilder.CursorPosition);
                var cursorOffsetX = (int)_textElement.Font.MeasureString(preCursor, _textElement.Scale).X;
                _cursorPositionX = cursorOffsetX;

                var delta = (float)gameTime.TotalGameTime.TotalMilliseconds / 2;
                _cursorAlpha = (float)MathHelpers.SinInterpolation(0.1f, 0.5f, delta)*2;
            }
        }

        protected override void OnKeyInput(char character, Keys key)
        {
            if (IsFocused)
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

	            if (_textBuilder.Length != originalLength)
	            {
		            Value = _textBuilder.Text;
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
            if (IsFocused)
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
    }
}
