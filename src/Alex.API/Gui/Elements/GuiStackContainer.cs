using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Alex.API.Gui.Layout;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements
{
    public class GuiStackContainer : GuiContainer
    {
        private Orientation _orientation = Orientation.Vertical;

        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;
                InvalidateLayout();
            }
        }

        private Alignment _childAnchor = Alignment.TopCenter;

        public Alignment ChildAnchor
        {
            get => _childAnchor;
            set
            {
                _childAnchor = value;
                UpdateLayoutAlignments();
            }
        }

        public int Spacing = 0;

        public GuiStackContainer()
        {

        }

        protected override Size MeasureChildrenCore(Size availableSize, IReadOnlyCollection<GuiElement> children)
        {
            var containerSize = availableSize;

            var size = Size.Zero;
            Thickness lastOffset = Thickness.Zero;

            foreach (var child in children)
            {
                containerSize += lastOffset;

                var childSize = child.Measure(containerSize) - child.Margin;
                
                var offset = CalculateOffset(ChildAnchor, childSize, child.Margin);
                
                size += offset;
                containerSize -= offset;

                lastOffset = CalculateOffset(ChildAnchor, Size.Zero, child.Margin);
            }

            size += lastOffset;

            return size;
        }

        private Thickness CalculateOffset(Alignment alignment, Size size, Thickness margin)
        {
            var offset = Thickness.Zero;

            if (Orientation == Orientation.Vertical)
            {
                if (alignment.HasFlag(Alignment.MinY))
                {
                    offset.Top += size.Height + margin.Bottom;
                }
                else if (alignment.HasFlag(Alignment.MaxY))
                {
                    offset.Bottom += size.Height + margin.Top;
                }
            }
            else if (Orientation == Orientation.Horizontal)
            {
                if (alignment.HasFlag(Alignment.MinX))
                {
                    offset.Left += size.Width + margin.Right;
                }
                else if (alignment.HasFlag(Alignment.MaxX))
                {
                    offset.Right += size.Width + margin.Left;
                }
            }

            return offset;
        }

        protected override void ArrangeChildrenCore(Rectangle finalRect, IReadOnlyCollection<GuiElement> children)
        {
            var positioningBounds = finalRect;

            var alignment = ChildAnchor;

            var offset = Padding;
            var lastOffset = Thickness.Zero;

            foreach (var child in children)
            {
                //offset -= lastOffset;

                var layoutBounds = PositionChild(child, alignment, positioningBounds, Thickness.Max(Padding, lastOffset), offset, true);

                var currentOffset = CalculateOffset(alignment, layoutBounds.Size, layoutBounds.Margin);

                offset += currentOffset;
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

