using System;
using System.Collections.Generic;
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

        public GuiStackContainer()
        {

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

            foreach (var child in children)
            {
                containerSize += lastOffset;

                var childSize = child.Measure(new Size(widthOverride == 0 ? containerSize.Width : widthOverride, 
                                                       heightOverride == 0 ? containerSize.Height : heightOverride)) - child.Margin;
                
                var offset = CalculateOffset(alignment, childSize, child.Margin);
                lastOffset = CalculateOffset(alignment, Size.Zero, child.Margin);

                if (Orientation == Orientation.Vertical)
                {
                    size.Width = Math.Max(size.Width, childSize.Width - lastOffset.Horizontal);
                    size.Width = Math.Max(size.Width, childSize.Width - lastOffset.Horizontal);
                    size.Height += offset.Vertical;

                    containerSize.Height -= offset.Vertical;
                }
                else if (Orientation == Orientation.Horizontal)
                {
                    size.Width += offset.Horizontal;
                    size.Height = Math.Max(size.Height, childSize.Height - lastOffset.Vertical);
                    size.Height = Math.Max(size.Height, childSize.Height - lastOffset.Vertical);

                    containerSize.Width -= offset.Horizontal;
                }


            }

            //size -= lastOffset;

            return size;
        }
        
        private Thickness CalculateOffset(Alignment alignment, Size size, Thickness margin)
        {
            var offset = Thickness.Zero;

            var vertical   = (alignment & (Alignment.OrientationY));
            var horizontal = (alignment & (Alignment.OrientationX));

            if (Orientation == Orientation.Vertical)
            {
                if((vertical & Alignment.MinY) != 0)
                {
                    offset.Top += size.Height + margin.Bottom;
                }
                else if((vertical & Alignment.MaxY) != 0)
                {
                    offset.Bottom += size.Height + margin.Top;
                }
                else if ((vertical & Alignment.FillY) != 0)
                {
                    offset.Top += size.Height + margin.Bottom;
                }
            }
            else if (Orientation == Orientation.Horizontal)
            {
                if((horizontal & Alignment.MinX) != 0)
                {
                    offset.Left += size.Width + margin.Right;
                }
                else if((horizontal & Alignment.MaxX) != 0)
                {
                    offset.Right += size.Width + margin.Left;
                }
                else if ((horizontal & Alignment.FillX) != 0)
                {
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
            var positioningBounds = finalRect;

            var alignment = NormalizeAlignmentForArrange(Orientation, ChildAnchor);

            var offset = Padding;
            var lastOffset = Thickness.Zero;

            foreach (var child in children)
            {
                //offset -= lastOffset;

                var layoutBounds = PositionChild(child, alignment, positioningBounds, Thickness.Max(Padding, lastOffset), offset, true);

                var currentOffset = CalculateOffset(alignment, layoutBounds.Size, layoutBounds.Margin);

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

                lastOffset = CalculateOffset(alignment, Size.Zero, layoutBounds.Margin);
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
            element.Anchor = _childAnchor;
            InvalidateLayout();
        }
    
    }
}

