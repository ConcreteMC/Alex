using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Attributes;
using Alex.API.Gui.Elements.Controls;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Layout
{
	public class GuiScrollableStackContainer : GuiStackContainer
	{
		protected const int ScrollBarSize = 5;

		[DebuggerVisible] public ScrollMode VerticalScrollMode   { get; set; } = ScrollMode.Auto;
		[DebuggerVisible] public ScrollMode HorizontalScrollMode { get; set; } = ScrollMode.Auto;

		private                  bool  _hasVerticalScroll;
		private                  bool  _hasHorizontalScroll;

		[DebuggerVisible]
		public Point ScrollOffset
		{
			get => _scrollOffset;
			set
			{
				_scrollOffset = value;
				InvalidateLayout();
			}
		}

		protected GuiScrollBar VerticalScrollBar;
		protected GuiScrollBar HorizontalScrollBar;
		private Point _scrollOffset = Point.Zero;


		public GuiScrollableStackContainer()
		{

			AddChild(VerticalScrollBar = new GuiScrollBar()
			{
				Orientation = Orientation.Vertical,
				Anchor      = Alignment.FillRight,
				Width       = 10,
				MaxWidth    = 10
			});
			AddChild(HorizontalScrollBar = new GuiScrollBar()
			{
				Orientation = Orientation.Horizontal,
				Anchor      = Alignment.BottomFill,
				Height      = 10,
				MaxHeight   = 10
			});

			VerticalScrollBar.ScrollOffsetValueChanged   += VerticalScrollBarOnScrollOffsetValueChanged;
			HorizontalScrollBar.ScrollOffsetValueChanged += HorizontalScrollBarOnScrollOffsetValueChanged;
		}

		private void HorizontalScrollBarOnScrollOffsetValueChanged(object sender, ScrollOffsetValueChangedEventArgs e)
		{
			ScrollOffset = new Point(e.ScrollOffsetValue, ScrollOffset.Y);
		}

		private void VerticalScrollBarOnScrollOffsetValueChanged(object sender, ScrollOffsetValueChangedEventArgs e)
		{
			ScrollOffset = new Point(ScrollOffset.X, e.ScrollOffsetValue);
		}
		protected override void OnAfterMeasure()
		{
			base.OnAfterMeasure();
			var sizeDiff = ContentSize - RenderBounds.Size;

			VerticalScrollBar.MaxScrollOffset   = Math.Max(0, sizeDiff.Height);
			HorizontalScrollBar.MaxScrollOffset = Math.Max(0, sizeDiff.Width);

		}

		protected override void OnUpdate(GameTime gameTime)
		{
			UpdateScroll();

			base.OnUpdate(gameTime);
		}

		protected virtual void UpdateScroll()
		{
			if (VerticalScrollMode == ScrollMode.Auto)
			{
				_hasVerticalScroll = ContentSize.Height > RenderBounds.Height;
			}
			else if (VerticalScrollMode == ScrollMode.Hidden)
			{
				_hasVerticalScroll = false;
			}
			else if (VerticalScrollMode == ScrollMode.Visible)
			{
				_hasVerticalScroll = true;
			}

			VerticalScrollBar.IsVisible = _hasVerticalScroll;

			if (HorizontalScrollMode == ScrollMode.Auto)
			{
				_hasHorizontalScroll = ContentSize.Width > RenderBounds.Width;
			}
			else if (HorizontalScrollMode == ScrollMode.Hidden)
			{
				_hasHorizontalScroll = false;
			}
			else if (HorizontalScrollMode == ScrollMode.Visible)
			{
				_hasHorizontalScroll = true;
			}

			HorizontalScrollBar.IsVisible = _hasHorizontalScroll;

			ScrollOffset = new Point(HorizontalScrollBar.ScrollOffsetValue, VerticalScrollBar.ScrollOffsetValue);
		}

		protected override void ArrangeChildrenCore(Rectangle finalRect, IReadOnlyCollection<GuiElement> children)
		{
			//var contentRect = new Rectangle(finalRect.Location  - new Thickness(ScrollOffset.X, ScrollOffset.Y, 0, 0), finalRect.Size + new Thickness(ScrollOffset.X, ScrollOffset.Y, 0, 0));
			base.ArrangeChildrenCore(finalRect, children);
			
			if (HorizontalScrollBar != null)
			{
				PositionChild(HorizontalScrollBar, Alignment.BottomFill, finalRect, Thickness.Zero,
							  Thickness.Zero,              true);
			}

			if (VerticalScrollBar != null)
			{
				PositionChild(VerticalScrollBar, Alignment.FillRight, finalRect, Thickness.Zero,
							  Thickness.Zero,            true);
			}

			ForEachChild(c => ((GuiElement)c).RenderTransform = Matrix.CreateTranslation(-ScrollOffset.X, -ScrollOffset.Y, 0));
		}

		protected override Size MeasureChildrenCore(Size availableSize, IReadOnlyCollection<GuiElement> children)
		{
			var containerSize = availableSize;

			var alignment = ChildAnchor;

			int widthOverride = 0, heightOverride = 0;

			if (Orientation == Orientation.Horizontal && (alignment & Alignment.FillX) != 0)
			{
				widthOverride = (int) (availableSize.Width / (float) children.Count);
			}

			if (Orientation == Orientation.Vertical && (alignment & Alignment.FillY) != 0)
			{
				heightOverride = (int) (availableSize.Height / (float) children.Count);
			}

			var baseVal = base.MeasureChildrenCore(availableSize, children);

			if (HorizontalScrollBar != null)
			{
				HorizontalScrollBar.Measure(new Size(widthOverride == 0 ? containerSize.Width : widthOverride,
													 heightOverride == 0 ? containerSize.Height : heightOverride));
			}

			if (VerticalScrollBar != null)
			{
				VerticalScrollBar.Measure(new Size(widthOverride == 0 ? containerSize.Width : widthOverride,
												   heightOverride == 0 ? containerSize.Height : heightOverride));
			}

			return baseVal;
		}

		protected override void OnChildAdded(IGuiElement element)
		{
			base.OnChildAdded(element);

			if (element != HorizontalScrollBar && element != VerticalScrollBar)
			{
				element.Anchor = ChildAnchor;
			}
		}
	}
}
