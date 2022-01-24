using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class GuiDropdown : ValuedControl<int>
	{
		public Color BorderColor { get; set; } = Color.LightGray;

		public Thickness BorderThickness { get; set; } = Thickness.One;
		public List<string> Options { get; } = new List<string>();

		private Color _textColor = Color.White;

		public Color TextColor
		{
			get => _textColor;
			set
			{
				_textColor = value;
				UpdateDisplayText(Value);
			}
		}

		private TextElement _textElement;

		public GuiDropdown()
		{
			MinWidth = 100;
			MinHeight = 20;
			BackgroundOverlay = Color.Black;
			HighlightedBackground = Color.Gray;
			FocusedBackground = Color.Black * 0.8f;

			AddChild(_textElement = new TextElement() { Anchor = Alignment.MiddleLeft });
		}

		private void UpdateDisplayText(int value)
		{
			if (Options.Count == 0)
				return;

			value = Math.Clamp(value, 0, Options.Count - 1);

			_textElement.TextColor = _textColor;
			_textElement.Text = Options[value];
		}

		/// <inheritdoc />
		protected override void OnInit(IGuiRenderer renderer)
		{
			base.OnInit(renderer);
		}

		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);

			var bounds = RenderBounds;
			bounds.Inflate(1f, 1f);
			graphics.DrawRectangle(bounds, BorderColor, BorderThickness);

			var position = bounds.Location;

			//bounds = RenderBounds;
			if (Focused)
			{
				var cursorPosition = Mouse.GetState();
				int y = 0;

				for (var index = 0; index < Options.Count; index++)
				{
					var option = Options[index];

					y += bounds.Height;
					var pos = position + new Point(0, y);
					var rect = new Rectangle(pos, new Point(bounds.Width, bounds.Height));

					graphics.FillRectangle(rect, _higlightedIndex == index ? HighlightedBackground : FocusedBackground);
					graphics.DrawRectangle(rect, BorderColor, BorderThickness);
					graphics.DrawString(new Vector2(pos.X + 2, pos.Y + 2), option, _textColor, FontStyle.None, 1f);
				}
			}
		}

		private int _higlightedIndex = -1;
		private MouseState _previousMouseState = new MouseState();

		/// <inheritdoc />
		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);

			if (Focused)
			{
				var bounds = RenderBounds;
				bounds.Inflate(1f, 1f);

				var position = bounds.Location;
				//bounds = RenderBounds;

				var mouseState = Mouse.GetState();

				if (mouseState != _previousMouseState)
				{
					var isCursorDown = mouseState.LeftButton == ButtonState.Pressed;

					var cursorPosition = mouseState.Position;
					cursorPosition = GuiManager.GuiRenderer.Unproject(cursorPosition.ToVector2()).ToPoint();

					int y = 0;

					var previousHighlight = _higlightedIndex;

					for (var index = 0; index < Options.Count; index++)
					{
						//var option = Options[index];
						y += bounds.Height;
						var pos = position + new Point(0, y);
						var rect = new Rectangle(pos, new Point(bounds.Width, bounds.Height));

						if (rect.Contains(cursorPosition))
						{
							_higlightedIndex = index;

							if (mouseState.LeftButton == ButtonState.Released
							    && _previousMouseState.LeftButton == ButtonState.Pressed)
							{
								//Clicked item.
								Value = index;
								GuiManager.FocusManager.FocusedElement = null;

								//FocusContext.ClearFocus(this);
							}

							break;
						}
					}

					if (_higlightedIndex != previousHighlight) { }

					_previousMouseState = mouseState;
				}
			}
		}

		/// <inheritdoc />
		protected override void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown)
		{
			base.OnCursorMove(cursorPosition, previousCursorPosition, isCursorDown);
		}

		/// <inheritdoc />
		protected override bool OnValueChanged(int value)
		{
			UpdateDisplayText(value);

			return base.OnValueChanged(value);
		}
	}
}