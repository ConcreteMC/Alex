using System;
using System.Collections.Generic;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Layout
{
    public class GuiStackContainer : GuiContainer
    {
        private Orientation _orientation = Orientation.Vertical;
        private Alignment _childAnchor = Alignment.TopCenter;

        public virtual Orientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                InvalidateLayout();
            }
        }


        public Alignment ChildAnchor
        {
            get => _childAnchor;
            set
            {
                _childAnchor = value;
                UpdateLayoutAlignments();
            }
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
            

            var size = Size.Zero;
            Thickness lastOffset = Thickness.Zero;

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

            foreach (var child in children)
            {
				if(child == HorizontalScrollBar || child == VerticalScrollBar) continue;
				
                containerSize += lastOffset;

                var thisOffset = CalculateOffset(alignment, Size.Zero, child.Margin, lastOffset);

                var childSize = child.Measure(new Size(widthOverride == 0 ? containerSize.Width : widthOverride, 
                                                       heightOverride == 0 ? containerSize.Height : heightOverride)) - thisOffset;
                
                var offset = CalculateOffset(alignment, childSize, child.Margin, lastOffset);

                if (Orientation == Orientation.Vertical)
                {
                    size.Width = Math.Max(size.Width, childSize.Width);
                    size.Height += childSize.Height;

                    containerSize.Height -= offset.Vertical;
                }
                else if (Orientation == Orientation.Horizontal)
                {
                    size.Width += childSize.Width;
                    size.Height = Math.Max(size.Height, childSize.Height);

                    containerSize.Width -= offset.Horizontal;
                }

                lastOffset = thisOffset;
            }

            size -= lastOffset;

            return size;
        }

	    protected Thickness CalculateOffset(Alignment alignment, Size size, Thickness margin, Thickness previousMargin)
        {
            var offset = Thickness.Zero;

            var vertical   = (alignment & (Alignment.OrientationY));
            var horizontal = (alignment & (Alignment.OrientationX));

            if (Orientation == Orientation.Vertical)
            {
                if((vertical & Alignment.MinY) != 0)
                {
                    offset.Top -= Math.Min(previousMargin.Bottom, margin.Top);
                    offset.Top += size.Height + margin.Bottom;
                }
                else if((vertical & Alignment.MaxY) != 0)
                {
                    offset.Bottom -= Math.Min(previousMargin.Top, margin.Bottom);
                    offset.Bottom += size.Height + margin.Top;
                }
                else if ((vertical & Alignment.FillY) != 0)
                {
                    offset.Top -= Math.Min(previousMargin.Bottom, margin.Top);
                    offset.Top += size.Height + margin.Bottom;
                }
            }
            else if (Orientation == Orientation.Horizontal)
            {
                if((horizontal & Alignment.MinX) != 0)
                {
                    offset.Left -= Math.Min(previousMargin.Right, margin.Left);
                    offset.Left += size.Width + margin.Right;
                }
                else if((horizontal & Alignment.MaxX) != 0)
                {
                    offset.Right -= Math.Min(previousMargin.Left, margin.Right);
                    offset.Right += size.Width + margin.Left;
                }
                else if ((horizontal & Alignment.FillX) != 0)
                {
                    offset.Left -= Math.Min(previousMargin.Right, margin.Left);
                    offset.Left += size.Width + margin.Right;
                }
            }

            return offset;
        }

        public static Alignment NormalizeAlignmentForArrange(Orientation orientation, Alignment alignment)
        {
            var vertical = (alignment & (Alignment.OrientationY));
            var horizontal = (alignment & (Alignment.OrientationX));

            if (orientation == Orientation.Vertical)
            {
                if((vertical & Alignment.FillY) != 0)
                {
                    vertical = Alignment.MinY;
				}
				else if ((vertical & Alignment.CenterY) != 0)
                {
	                vertical = Alignment.MinY;
                }
				else if((vertical & Alignment.MaxY) != 0)
                {
                    vertical = Alignment.MaxY;
                }
                else
                // if((vertical & Alignment.MinY) != 0)
                {
                    vertical = Alignment.MinY;
                }
            }
            else if (orientation == Orientation.Horizontal)
            {
                if((horizontal & Alignment.FillX) != 0)
                {
                    horizontal = Alignment.MinX;
				}
				else if ((horizontal & Alignment.CenterX) != 0)
                {
	                horizontal = Alignment.MinX;
                }
				else if((horizontal & Alignment.MaxX) != 0)
                {
                    horizontal = Alignment.MaxX;
                }
                else
                // if((horizontal & Alignment.MinX) != 0)
                {
                    horizontal = Alignment.MinX;
                }
            }

            return (vertical | horizontal);
        }

        protected override void ArrangeChildrenCore(Rectangle finalRect, IReadOnlyCollection<GuiElement> children)
        {
			finalRect = new Rectangle(finalRect.Location - ScrollOffset, finalRect.Size);
            var positioningBounds = finalRect + Padding;

            var alignment = NormalizeAlignmentForArrange(Orientation, ChildAnchor);

	        var childSize = ContentSize;
			var offset = Padding;

	        if (ChildAnchor.HasFlag(Alignment.CenterX))
	        {
				offset.Left = Math.Max(Padding.Left, (int)((positioningBounds.Width - childSize.Width) / 2f));
			}

			if (ChildAnchor.HasFlag(Alignment.CenterY))
			{
				offset.Top = Math.Max(Padding.Top, (int)((positioningBounds.Height - childSize.Height) / 2f));
			}

			var lastOffset = Thickness.Zero;

			if (HorizontalScrollBar != null)
			{
				PositionChild(HorizontalScrollBar, HorizontalScrollBar.Anchor, positioningBounds, Thickness.Zero,
							  offset,              true);
			}
			
			if (VerticalScrollBar != null)
			{
				PositionChild(VerticalScrollBar, VerticalScrollBar.Anchor, positioningBounds, Thickness.Zero,
							  offset,              true);
			}

            foreach (var child in children)
            {
				if (child == HorizontalScrollBar || child == VerticalScrollBar)
				{
					continue;
				}
                //offset -= lastOffset;

                var layoutBounds = PositionChild(child, alignment, positioningBounds, lastOffset, offset, true);

                var currentOffset = CalculateOffset(alignment, layoutBounds.Size, layoutBounds.Margin, lastOffset);

                offset += currentOffset;

                //if (Orientation == Orientation.Vertical)
                //{
                //    size.Width  =  Math.Max(size.Width, childSize.Width - lastOffset.Horizontal);
                //    size.Height += offset.Vertical;
                //}
                //else if (Orientation == Orientation.Horizontal)
                //{
                //    size.Width  += offset.Horizontal;
                //    size.Height =  Math.Max(size.Height, childSize.Height - lastOffset.Vertical);
                //}
                lastOffset = CalculateOffset(alignment, Size.Zero, layoutBounds.Margin, lastOffset);
            }
        }

        private void UpdateLayoutAlignments()
        {
            ForEachChild(UpdateLayoutAlignment);
        }
        protected override void OnChildAdded(IGuiElement element)
        {
				UpdateLayoutAlignment(element);
        }
        private void UpdateLayoutAlignment(IGuiElement element)
		{
			if (element != HorizontalScrollBar && element != VerticalScrollBar)
			{
				element.Anchor = _childAnchor;
			}

			InvalidateLayout();
		}
    
		protected const int ScrollBarSize = 5;

		public ScrollMode VerticalScrollMode { get; set; } = ScrollMode.Auto;
		public ScrollMode HorizontalScrollMode { get; set; } = ScrollMode.Auto;

		private bool _hasVerticalScroll;
		private bool _hasHorizontalScroll;
		public Point ScrollOffset { get; set; } = Point.Zero;

		protected GuiScrollBar VerticalScrollBar;
		protected GuiScrollBar HorizontalScrollBar;

		public GuiStackContainer()
		{
				AddChild(VerticalScrollBar = new GuiScrollBar()
				{
					Orientation = Orientation.Vertical,
					Anchor      = Alignment.FillRight,
					Width = 10,
					MaxWidth = 10
				});
				AddChild(HorizontalScrollBar = new GuiScrollBar()
				{
					Orientation = Orientation.Horizontal,
					Anchor      = Alignment.BottomFill,
					Height = 10,
					MaxHeight = 10
				});
			
		}
		
		protected override void OnAfterMeasure()
		{
			UpdateScroll();
			base.OnAfterMeasure();
		}

		protected virtual void UpdateScroll()
		{
			if (VerticalScrollMode == ScrollMode.Auto)
			{
				_hasVerticalScroll = ContentSize.Height > Size.Height;
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
				_hasHorizontalScroll = ContentSize.Width > Size.Width;
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
		}
		
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);
		}
    }
}

