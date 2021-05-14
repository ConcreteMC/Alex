using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class GuiDropdown : RocketControl
	{
		public  Color BorderColor { get; set; } = Color.LightGray;

		public Thickness BorderThickness { get; set; } = Thickness.One;
		
		private int _selectedIndex = 0;
		public List<string> Options { get; } = new List<string>();

		public int SelectedIndex
		{
			get => _selectedIndex;
			set
			{
				if (value >= 0 && value < Options.Count)
				{
					_selectedIndex = value;
					
					UpdateDisplayText();
				}
			}
		}
		
		private Color _textColor = Color.White;
		public Color TextColor
		{
			get => _textColor;
			set
			{
				_textColor = value;
				UpdateDisplayText();
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
			
			AddChild(_textElement = new TextElement()
			{
				Anchor = Alignment.Fill
			});
		}

		private void UpdateDisplayText()
		{
			_textElement.TextColor = _textColor;
			_textElement.Text = Options[_selectedIndex];
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
			//if (Focused)
			{
				var cursorPosition = Mouse.GetState();
				int y = 0;

				foreach (var option in Options)
				{
					y += bounds.Height;
					var pos = position + new Point(0, y);
					var rect = new Rectangle(pos, new Point(bounds.Width, bounds.Height));

					graphics.FillRectangle(rect, FocusedBackground);
					graphics.DrawRectangle(rect, BorderColor, BorderThickness);
					graphics.DrawString(new Vector2(pos.X + 2, pos.Y + 2), option, _textColor, FontStyle.None, 1f);
				}
			}
		}
		
	}
}