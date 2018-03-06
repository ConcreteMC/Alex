using System;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Rendering;
using Alex.Graphics.UI.Themes;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI
{
	public class UiElement
	{
		public event EventHandler SizeChanged;
		public event EventHandler LayoutChanged;

		public UiContainer Container { get; internal set; }

		private Rectangle _outerBounds;

		public Rectangle OuterBounds
		{
			get { return _outerBounds; }
			private set
			{
				_outerBounds = value;
				MarkLayoutDirty();
			}
		}

		private Rectangle _bounds;

		public Rectangle Bounds
		{
			get { return _bounds; }
			private set
			{
				_bounds = value;
				MarkLayoutDirty();
			}
		}

		private Rectangle _clientBounds;

		public Rectangle ClientBounds
		{
			get { return _clientBounds; }
			private set
			{
				_clientBounds = value;
				MarkLayoutDirty();
			}
		}

		private Point _offset;

		public Point Offset
		{
			get { return _offset; }
			set
			{
				_offset = value;
				MarkLayoutDirty();
			}
		}

		private Point _position;

		public Point Position
		{
			get { return _position; }
			set
			{
				_position = value;
				MarkLayoutDirty();
			}
		}

		public Point Size => new Point(ActualWidth, ActualHeight);


		private int _actualWidth;

		public int ActualWidth
		{
			get { return _actualWidth; }
			protected internal set
			{
				_actualWidth = value;
				MarkLayoutDirty();
			}
		}

		private int _actualHeight;

		public int ActualHeight
		{
			get { return _actualHeight; }
			protected internal set
			{
				_actualHeight = value;
				MarkLayoutDirty();
			}
		}

		public bool Visible { get; set; } = true;

		public string ClassName { get; set; }

		private bool _isLayoutDirty = true;

		#region Styling Properties

		private UiElementStyle _style = new UiElementStyle();

		public UiElementStyle Style
		{
			get { return _style; }
			private set
			{
				_style = value;
				var customStyle = CustomStyle;

				if (customStyle != null)
				{
					UiTheme.MergeStyles(CustomStyle, _style);
				}

				MarkLayoutDirty();
			}
		}

		public UiElementStyle CustomStyle { get; } = new UiElementStyle();

		protected int? Width => Style.Width;
		protected int? Height => Style.Height;

		protected Thickness Margin => Style.Margin;
		protected Thickness Padding => Style.Margin;

		#endregion

		public UiElement(int? width, int? height) : this()
		{
			CustomStyle.Width = width;
			CustomStyle.Height = height;
		}

		public UiElement()
		{

		}


		public virtual void UpdateSize()
		{
			var autoSize = GetAutoSize();

			// Width
			SizeMode widthMode = Style.WidthSizeMode;
			if (widthMode == SizeMode.FitToContent)
			{
				ActualWidth = autoSize.X + Padding.Horizontal;
			}
			else if (widthMode == SizeMode.Absolute)
			{
				ActualWidth = Width.HasValue ? Width.Value : 0;
			}

			// Height
			SizeMode heightMode = Style.HeightSizeMode;
			if (heightMode == SizeMode.FitToContent)
			{
				ActualHeight = autoSize.Y + Padding.Vertical;
			}
			else if (heightMode == SizeMode.Absolute)
			{
				ActualHeight = Height.HasValue ? Height.Value : 0;
			}

			SizeChanged?.Invoke(this, null);
		}

		protected internal virtual Point GetAutoSize()
		{
			return new Point(Padding.Horizontal, Padding.Vertical);
		}

		public void UpdateLayout()
		{
			Position = (Container?.ClientBounds.Location ?? Point.Zero) + Offset;
			
			UpdateBounds();
			OnUpdateLayout();

			LayoutChanged?.Invoke(this, null);
		}

		protected void UpdateBounds()
		{
			var outerBounds = new Rectangle(Position.X, Position.Y, Size.X + Margin.Horizontal, Size.Y + Margin.Vertical);
			var bounds = outerBounds - Margin;
			var inner = bounds - Padding;

			OuterBounds = outerBounds;
			Bounds = bounds;
			ClientBounds = inner;
		}

		public void Draw(GameTime gameTime, UiRenderer renderer)
		{
			Style = renderer.Theme.GetCompiledStyleFor(this);

			OnDraw(gameTime, renderer);
		}

		public void Update(GameTime gameTime)
		{
			if (_isLayoutDirty)
			{
				UpdateBounds();
				UpdateLayout();

				_isLayoutDirty = false;
			}

			OnUpdate(gameTime);
		}

		protected virtual void OnDraw(GameTime gameTime, UiRenderer renderer)
		{
			renderer.DrawElement(this);
		}

		protected virtual void OnUpdateLayout()
		{

		}

		protected virtual void OnUpdate(GameTime gameTime)
		{

		}

		protected void MarkLayoutDirty()
		{
			if (_isLayoutDirty) return;
			_isLayoutDirty = true;
		}
	}
}
