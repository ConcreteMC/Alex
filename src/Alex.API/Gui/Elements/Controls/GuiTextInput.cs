using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TextCopy;

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

	    private string _placeHolder = string.Empty;

	    public string PlaceHolder
	    {
		    get { return _placeHolder; }
		    set
		    {
			    _placeHolder = value;
			    UpdateDisplayText();
		    }
	    }
		
		public bool IsPasswordInput { get; set; } = false;
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
		private KeyboardState _kbState;
		private double _lastKeyInputTime = 0d;
		private double _lastUpdate;
        protected override void OnUpdate(GameTime gameTime)
		{
			var ms = gameTime.TotalGameTime.TotalMilliseconds;
			_lastUpdate = ms;

			base.OnUpdate(gameTime);
			
	        if (Focused)
			{
					var kbState = Keyboard.GetState();
				if(_kbState != kbState || (ms - _lastKeyInputTime > 150))
				{
					_kbState = kbState;

					var keys    = kbState.GetPressedKeys();

					var isShift = keys.Contains(Keys.LeftShift) || keys.Contains(Keys.RightShift);
					var isCtrl = keys.Contains(Keys.LeftControl) || keys.Contains(Keys.RightControl);

					if (keys.Contains(Keys.Home))
					{
						_textBuilder.CursorPosition = 0;
					}
					else if (keys.Contains(Keys.End))
					{
						_textBuilder.CursorPosition = _textBuilder.Length;
					}
					else if (isCtrl)
					{
						if (keys.Contains(Keys.C))
						{
							// Clipboard Copy
							if (_textBuilder.SelectedText.Length > 0)
							{
								Clipboard.SetText(_textBuilder.SelectedText);
							}
						}
						else if (keys.Contains(Keys.V))
						{
							// Clipboard Paste
							var clipboardText = Clipboard.GetText();
							if (!string.IsNullOrEmpty(clipboardText))
							{
								_textBuilder.Append(clipboardText);
							}
						}
						else if (keys.Contains(Keys.A))
						{
							_textBuilder.SelectAll();
						}
					}
					else
					{

						if (isShift && (keys.Contains(Keys.Left) || keys.Contains(Keys.Right)))
						{
							_textBuilder.IsSelecting = true;
						}

						if (keys.Contains(Keys.Left))
						{
							if (!isShift)
							{
								_textBuilder.ClearSelection();
							}

							_textBuilder.CursorPosition--;
						}
						else if (keys.Contains(Keys.Right))
						{
							if (!isShift)
							{
								_textBuilder.ClearSelection();
							}

							_textBuilder.CursorPosition++;
						}
					}

					if (keys.Any())
					{
						_lastKeyInputTime = ms;
					}
				}


				if (IsPasswordInput)
	            {
		            var preCursor = _textElement.Text.Substring(0, _textBuilder.CursorPosition);
		            var cursorOffsetX = (int)_textElement.Font.MeasureString(preCursor, _textElement.Scale).X;
		            _cursorPositionX = cursorOffsetX;
				}
	            else
	            {
		            var preCursor = _textBuilder.Text.Substring(0, _textBuilder.CursorPosition);
		            var cursorOffsetX = (int) _textElement.Font.MeasureString(preCursor, _textElement.Scale).X;
		            _cursorPositionX = cursorOffsetX;
	            }

	            var delta = (float)gameTime.TotalGameTime.TotalMilliseconds / 2;
                _cursorAlpha = (float)MathHelpers.SinInterpolation(0.1f, 0.5f, delta)*2;
            }
        }

        protected override bool OnKeyInput(char character, Keys key)
        {
			if (Focused)
			{
				if (key == Keys.Delete)
				{
					if (_textBuilder.Length != _textBuilder.CursorPosition)
					{
						_textBuilder.CursorPosition++;
						_textBuilder.RemoveCharacter();
					}
					else if (_textBuilder.SelectedText.Length > 0)
					{
						_textBuilder.RemoveSelection();
					}
				}
				else if(key == Keys.Back)
				{
					_textBuilder.RemoveCharacter();
					_lastKeyInputTime = _lastUpdate;
				}
				else if (!char.IsControl(character))
				{
					_textBuilder.AppendCharacter(character);
					_lastKeyInputTime = _lastUpdate;
				}
			}

	        return false;
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
				var textElementBounds = _textElement.RenderBounds;
				if (_textBuilder.HasSelection)
				{
					var startX = textElementBounds.X + _textBuilder.SelectionStartPosition;
					var endX = textElementBounds.X + _textBuilder.SelectionEndPosition;

					var widthUntilSelectionStart =
						_textElement.Font.MeasureString(_textBuilder
														.Text.Substring(0, _textBuilder.SelectionStartPosition));

					var selectedTextSize = _textElement.Font.MeasureString(_textBuilder.SelectedText, _textElement.Scale);
					var sizeX = 1 + selectedTextSize.X;

					graphics.FillRectangle(new Rectangle(textElementBounds.X + 1 + (int)widthUntilSelectionStart.X, textElementBounds.Y, (int)sizeX, textElementBounds.Height), _textColor.ForegroundColor);
				}

//                if (gameTime.TotalGameTime.Seconds % 2 < 1)
                {
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
			    if (IsPasswordInput)
			    {
				    value = new string('*', value.Length);
			    }
			    _textElement.Text = value;
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
