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
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            var text = _textBuilder.Text;
            _textElement.Text = text;
            
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
	            string originalValue = Value?.Clone().ToString() ?? string.Empty;
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

	            if (originalValue != _textBuilder.Text)
	            {
		            Value = _textBuilder.Text;
	            }
            }
        }

        protected override void OnDraw(GuiRenderArgs args)
        {
            base.OnDraw(args);

            var bounds = RenderBounds;
            bounds.Inflate(1f, 1f);
            args.DrawRectangle(bounds, BorderColor, BorderThickness);

            // Text
            if (IsFocused)
            {
                if (args.GameTime.TotalGameTime.Seconds % 2 == 0)
                {
                    var textElementBounds = _textElement.RenderBounds;
                    var offsetX = textElementBounds.X + _cursorPositionX + 1;

                    args.DrawLine(offsetX, textElementBounds.Y, offsetX, textElementBounds.Height, Color.White * _cursorAlpha);
                }
            }

        }
    }
}
