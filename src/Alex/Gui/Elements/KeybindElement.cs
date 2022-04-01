using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;
using RocketUI.Attributes;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace Alex.Gui.Elements
{
	public class KeybindElement : RocketControl, IValuedControl<Keys[]>
	{
		public static readonly Keys[] Unbound = new Keys[] { (Keys)int.MaxValue };

		private TextElement TextElement { get; }
		private TextElement TimerText { get; }

		[DebuggerVisible]
		public Color BorderColor
		{
			get => _borderColor;
			set
			{
				_borderColor = value;
				UpdateColors();
			}
		}

		[DebuggerVisible]
		public Thickness BorderThickness
		{
			get => _borderThickness;
			set
			{
				_borderThickness = value;
				UpdateColors();
			}
		}

		private Color _textColor;

		[DebuggerVisible]
		public Color TextColor
		{
			get => _textColor;
			set
			{
				_textColor = value;
				UpdateColors();
			}
		}

		[DebuggerVisible] public InputCommand InputCommand { get; }
		public int MaxKeys { get; set; } = 2;

		public bool ReadOnly
		{
			get => _readOnly;
			set
			{
				_readOnly = value;
				Enabled = !value;

				if (value)
				{
					AddClass("ReadOnly");
				}
				else
				{
					RemoveClass("ReadOnly");
				}
			}
		}

		public KeybindElement(IInputListener inputListener, InputCommand inputCommand, params Keys[] key)
		{
			InputCommand = inputCommand;
			_inputListener = inputListener;
			_value = key;

			BackgroundOverlay = Color.Black;

			TextElement = new TextElement();
			TextElement.Anchor = Alignment.MiddleCenter;
			TextElement.TextColor = TextColor;

			TimerText = new TextElement();
			TimerText.Anchor = Alignment.MiddleRight;
			TimerText.IsVisible = false;
			TimerText.TextColor = TextColor;

			AddChild(TextElement);
			AddChild(TimerText);

			UpdateText(key);
		}

		private List<Keys> _tempBinding = new List<Keys>();
		private TimeSpan _timer = TimeSpan.Zero;

		public bool IsChanging { get; private set; }

		private void FocusLost(bool unbind = false)
		{
			RemoveClass("Focused");
			IsChanging = false;
			TimerText.IsVisible = false;
			TextElement.TextOpacity = 1f;
			var newValue = _tempBinding.ToArray();
			_tempBinding.Clear();

			var remainingTime = _timer;
			_timer = TimeSpan.Zero;

			if (newValue.Length > 0)
			{
				Value = newValue;
			}
			else if (unbind)
			{
				Value = Unbound;
			}

			UpdateText(_value);
			/*else if (Value != Unbound)
			{
			    Value = Unbound;
			}*/
		}

		private bool _wasFocused = false;

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			if (_readOnly) return;

			bool focused = Focused;

			if (focused && !_wasFocused)
			{
				AddClass("Focused");
				IsChanging = true;
				_tempBinding.Clear();
				_timer = TimeSpan.FromSeconds(5);
				TextElement.Text = "_";
				TimerText.IsVisible = true;
			}
			else if (!focused && _wasFocused)
			{
				FocusLost();
			}

			if (_timer > TimeSpan.Zero)
			{
				_timer -= gameTime.ElapsedGameTime;
				TimerText.Text = $"{_timer:%s}";

				TextElement.TextOpacity = 1f - ((float)gameTime.TotalGameTime.TotalSeconds % 0.75f);

				if (_timer <= TimeSpan.Zero)
				{
					FocusLost();
				}
				else
				{
					var tempBinding = _tempBinding;

					if (tempBinding.Count > 0)
					{
						var keyboardState = Keyboard.GetState();

						if (!tempBinding.All(keyboardState.IsKeyDown))
						{
							FocusLost();
						}
					}
				}
			}

			_wasFocused = focused;
		}

		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);

			var bounds = RenderBounds;
			bounds.Inflate(1f, 1f);
			graphics.DrawRectangle(bounds, BorderColor, BorderThickness);
		}

		protected override bool OnKeyInput(char character, Keys key)
		{
			if (_readOnly)
				return false;

			if (Focused && key == Keys.Escape)
			{
				FocusLost(true);

				return true;
			}

			if (_timer <= TimeSpan.Zero || _tempBinding.Contains(key))
				return true;

			_timer = TimeSpan.FromSeconds(5);
			_tempBinding.Add(key);
			UpdateText(_tempBinding);

			if (_tempBinding.Count == MaxKeys)
			{
				FocusLost();
			}
			// if (Focused)
			// {
			//Value = key;

			//ClearFocus();
			// return true;
			// }

			return true;
		}

		private void UpdateText(ICollection<Keys> key)
		{
			if (key == Unbound)
			{
				AddClass("Unbound");
				TextElement.Text = $"Unbound";
			}
			else
			{
				RemoveClass("Unbound");
				TextElement.Text = string.Join(" + ", key); // key.ToString().SplitPascalCase();
			}
		}

		private void UpdateColors()
		{
			TextElement.TextColor = _textColor;
			TimerText.TextColor = _textColor;
		}

		public event EventHandler<Keys[]> ValueChanged;
		private readonly IInputListener _inputListener;
		private Keys[] _value;
		private bool _readOnly = false;
		private Color _borderColor = Color.LightGray;
		private Thickness _borderThickness = Thickness.One;

		public Keys[] Value
		{
			get { return _value; }
			set
			{
				var oldValue = _value;
				_value = value;

				UpdateText(value);

				if (_value != oldValue)
				{
					ValueChanged?.Invoke(this, _value);
				}
			}
		}

		public ValueFormatter<Keys[]> DisplayFormat { get; set; }
	}
}