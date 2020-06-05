using System.Linq;
using Alex.API.Gui.Graphics;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

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

	    protected TextInputBuilder TextBuilder;

        protected GuiTextElement TextElement;

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

	    
	    /// <summary>
	    /// 	Max characters in field.
	    /// 	Set to -1 for infinite.
	    /// </summary>
	    public int MaxCharacters { get; set; } = -1;
		
		public bool IsPasswordInput { get; set; } = false;
        public GuiTextInput(string text = null)
        {
            MinWidth = 100;
            MinHeight = 20;
            BackgroundOverlay = Color.Black;

            TextBuilder = new TextInputBuilder(text);
            AddChild(TextElement = new GuiTextElement()
            {
                Anchor = Alignment.MiddleLeft,
				Text = TextBuilder.Text,
				Enabled = false
            });

	        TextBuilder.TextChanged += (s, v) => Value = v;
	        Value = TextBuilder.Text;


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
						TextBuilder.CursorPosition = 0;
					}
					else if (keys.Contains(Keys.End))
					{
						TextBuilder.CursorPosition = TextBuilder.Length;
					}
					else if (isCtrl)
					{
						if (keys.Contains(Keys.C))
						{
							// Clipboard Copy
							if (TextBuilder.SelectedText.Length > 0)
							{
								Clipboard.SetText(TextBuilder.SelectedText);
							}
						}
						else if (keys.Contains(Keys.V))
						{
							// Clipboard Paste
							var clipboardText = Clipboard.GetText();
							if (!string.IsNullOrEmpty(clipboardText))
							{
								TextBuilder.Append(clipboardText.StripIllegalCharacters(TextElement.Font));
							}
						}
						else if (keys.Contains(Keys.A))
						{
							TextBuilder.SelectAll();
						}
					}
					else
					{

						if (isShift && (keys.Contains(Keys.Left) || keys.Contains(Keys.Right)))
						{
							TextBuilder.IsSelecting = true;
						}

						if (keys.Contains(Keys.Left))
						{
							if (!isShift)
							{
								TextBuilder.ClearSelection();
							}

							TextBuilder.CursorPosition--;
						}
						else if (keys.Contains(Keys.Right))
						{
							if (!isShift)
							{
								TextBuilder.ClearSelection();
							}

							TextBuilder.CursorPosition++;
						}
					}

					if (keys.Any())
					{
						_lastKeyInputTime = ms;
					}
				}


				if (IsPasswordInput)
	            {
		            var preCursor = TextElement.Text.Substring(0, TextBuilder.CursorPosition);
		            var cursorOffsetX = (int)TextElement.Font.MeasureString(preCursor, TextElement.Scale).X;
		            _cursorPositionX = cursorOffsetX;
				}
	            else
	            {
		            var preCursor = TextBuilder.Text.Substring(0, TextBuilder.CursorPosition);
		            var cursorOffsetX = (int) TextElement.Font.MeasureString(preCursor, TextElement.Scale).X;
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
					if (TextBuilder.Length != TextBuilder.CursorPosition)
					{
						TextBuilder.CursorPosition++;
						TextBuilder.RemoveCharacter();
					}
					else if (TextBuilder.SelectedText.Length > 0)
					{
						TextBuilder.RemoveSelection();
					}
				}
				else if(key == Keys.Back)
				{
					TextBuilder.RemoveCharacter();
					_lastKeyInputTime = _lastUpdate;
				}
				else if (!char.IsControl(character))
				{
					if (TextBuilder.Length < MaxCharacters || MaxCharacters == -1)
					{
						TextBuilder.AppendCharacter(character);
					}

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
				var textElementBounds = TextElement.RenderBounds;
				if (TextBuilder.HasSelection)
				{
					var startX = textElementBounds.X + TextBuilder.SelectionStartPosition;
					var endX = textElementBounds.X + TextBuilder.SelectionEndPosition;

					var widthUntilSelectionStart =
						TextElement.Font.MeasureString(TextBuilder
														.Text.Substring(0, TextBuilder.SelectionStartPosition));

					var selectedTextSize = TextElement.Font.MeasureString(TextBuilder.SelectedText, TextElement.Scale);
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
			    TextElement.TextColor = InactiveTextColor;

			    if (!string.IsNullOrWhiteSpace(PlaceHolder))
			    {
				    TextElement.Text = PlaceHolder;
			    }
			    else
			    {
				    TextElement.Text = " ";
			    }
		    }
		    else
		    {
			    TextElement.TextColor = Focused ? TextColor : InactiveTextColor;
			    if (IsPasswordInput)
			    {
				    value = new string('*', value.Length);
			    }
			    TextElement.Text = value;
		    }
	    }
		
	    protected override bool OnValueChanged(string value)
	    {
		    if (!TextBuilder.Text.Equals(value))
		    {
			    TextBuilder.Text = value;
		    }

		    UpdateDisplayText(value);
		    return true;
	    }
    }
}
