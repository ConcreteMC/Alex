using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Alex.API.Gui.Attributes;
using Alex.API.Gui.Elements.Controls;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.API.Gui.Elements.Layout
{
	public class GuiScrollableStackContainer : GuiStackContainer
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
		
		[DebuggerVisible] public ScrollMode VerticalScrollMode   { get; set; } = ScrollMode.Auto;
		[DebuggerVisible] public ScrollMode HorizontalScrollMode { get; set; } = ScrollMode.Auto;

		private                  bool  _hasVerticalScroll;
		private                  bool  _hasHorizontalScroll;

		[DebuggerVisible]
		public Vector2 ScrollOffset
		{
			get => _scrollOffset;
			set
			{
				if (value == _scrollOffset) return;

				var prevValue = _scrollOffset;
				_scrollOffset = value;
				OnScrollOffsetChanged(prevValue, _scrollOffset);
				InvalidateLayout();
			}
		}

		protected GuiScrollBar VerticalScrollBar;
		protected GuiScrollBar HorizontalScrollBar;
		private Vector2 _scrollOffset = Vector2.Zero;


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

			ClipToBounds = true;
		}

		private void HorizontalScrollBarOnScrollOffsetValueChanged(object sender, ScrollOffsetValueChangedEventArgs e)
		{
			ScrollOffset = new Vector2(e.ScrollOffsetValue, ScrollOffset.Y);
		}

		private void VerticalScrollBarOnScrollOffsetValueChanged(object sender, ScrollOffsetValueChangedEventArgs e)
		{
			ScrollOffset = new Vector2(ScrollOffset.X, e.ScrollOffsetValue);
		}
		

		private Matrix _childRenderTransform = Matrix.Identity;

		private void OnScrollOffsetChanged(Vector2 oldValue, Vector2 newValue)
		{
			//var prevTransform = _childRenderTransform;

			//if (newValue == Vector2.Zero)
			//{
			//	_childRenderTransform = Matrix.Identity;
			//}
			//else
			//{
			//	_childRenderTransform = Matrix.Identity * Matrix.CreateTranslation(-newValue.X, -newValue.Y, 0);
			//}

			//ForEachChild(e =>
			//{
			//	if(e is GuiElement guiElement)
			//		guiElement.RenderTransform = _childRenderTransform;
			//});

			//Log.Info($"{GetType().Name}.ScrollOffset.Changed {{ScrollOffset=({oldValue} => {newValue}), RenderTransform=({prevTransform} => {_childRenderTransform})}}");
			//Debug.WriteLine($"{GetType().Name}.ScrollOffset.Changed {{ScrollOffset=({oldValue} => {newValue}), RenderTransform=({prevTransform} => {_childRenderTransform})}}");
			Log.Info($"{GetType().Name}.ScrollOffset.Changed {{ScrollOffset=({oldValue} => {newValue})}}");
			Debug.WriteLine($"{GetType().Name}.ScrollOffset.Changed {{ScrollOffset=({oldValue} => {newValue})}}");
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

			ScrollOffset = new Vector2(HorizontalScrollBar.ScrollOffsetValue, VerticalScrollBar.ScrollOffsetValue);
		}

		protected override void ArrangeChildrenCore(Rectangle finalRect, IReadOnlyCollection<GuiElement> children)
		{
			//var contentRect = new Rectangle(finalRect.Location  - new Thickness((int) ScrollOffset.X, (int)ScrollOffset.Y, 0, 0), finalRect.Size + new Thickness((int)ScrollOffset.X, (int)ScrollOffset.Y, 0, 0));
			var contentRect = new Rectangle(finalRect.Location, Size.Max(finalRect.Size, ContentSize));
			contentRect.Offset(-ScrollOffset);

			base.ArrangeChildrenCore(contentRect, children);
			
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

			//ForEachChild(c => ((GuiElement)c).RenderTransform = Matrix.CreateTranslation(-ScrollOffset.X, -ScrollOffset.Y, 0));
		}

		protected override bool ShouldPositionChild(GuiElement child)
		{
			if (child == HorizontalScrollBar || child == VerticalScrollBar) return false;

			return base.ShouldPositionChild(child);
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
