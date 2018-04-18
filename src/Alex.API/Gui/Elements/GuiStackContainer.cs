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

        protected override Size MeasureChildren(Size availableSize)
        {
            var children = Children.Cast<GuiElement>().ToArray();

            var containerSize = availableSize + Padding;

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

            _offset = Thickness.Zero;
            foreach (var child in children)
            {
                var layoutBounds = PositionChild(child, alignment, positioningBounds, _offset, true);
                
                _offset += CalculateOffset(alignment, layoutBounds.Size, child.Margin);
            }
        }

        private Thickness _offset = Thickness.Zero;

        //  protected override void PositionChildCore(GuiElement child, ref LayoutBoundingRectangle bounds, Alignment alignment)
        //  {
        //   var mask = Orientation == Orientation.Vertical ? Alignment.OrientationY : Alignment.OrientationX;

        //   alignment = (ChildAnchor & mask);

        //   if (Orientation == Orientation.Vertical)
        //   {
        //    if (alignment.HasFlag(Alignment.MinY))
        //    {
        //	    bounds.AnchorTop = _offset.Top;
        //    }
        //    else if (alignment.HasFlag(Alignment.MaxY))
        //    {
        //		bounds.Offset(null, null, null, _offset.Bottom);
        //	    bounds.AnchorBottom = _offset.Bottom;
        //    }

        //   }
        //   else if (Orientation == Orientation.Horizontal)
        //   {
        //    if (alignment.HasFlag(Alignment.MinX))
        //    {
        //	    bounds.AnchorLeft = _offset.Left;
        //    }
        //    else if (alignment.HasFlag(Alignment.MaxX))
        //    {
        //	    bounds.AnchorRight = _offset.Right;
        //    }
        //   }

        //base.PositionChildCore(child, ref bounds, alignment);

        //   var childBounds = bounds.Bounds;

        //   if (Orientation == Orientation.Vertical)
        //   {
        //    if (alignment.HasFlag(Alignment.MinY))
        //    {
        //	    _offset.Top += childBounds.Height + Spacing;
        //    }
        //    else if (alignment.HasFlag(Alignment.MaxY))
        //    {
        //	    _offset.Bottom += childBounds.Height + Spacing;
        //    }
        //   }
        //   else if (Orientation == Orientation.Horizontal)
        //   {
        //    if (alignment.HasFlag(Alignment.MinX))
        //    {
        //	    _offset.Left += childBounds.Width + Spacing;
        //    }
        //    else if (alignment.HasFlag(Alignment.MaxX))
        //    {
        //	    _offset.Right += childBounds.Width + Spacing;
        //    }
        //   }
        //  }

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

        //      protected override void OnUpdateLayout()
        //      {
        //          base.OnUpdateLayout();

        //          ForEachChild(c =>
        //          {
        //              if (VerticalContentAlignment != VerticalAlignment.None)
        //              {
        //                  c.VerticalAlignment = VerticalAlignment.None;
        //              }

        //              if (HorizontalContentAlignment != HorizontalAlignment.None)
        //              {
        //                  c.HorizontalAlignment = HorizontalAlignment.None;
        //              }
        //          });

        //          AlignContentVertically();
        //          AlignContentHorizontally();
        //      }


        //      protected void AlignContentVertically()
        //      {
        //          if (!HasChildren || VerticalContentAlignment == VerticalAlignment.None) return;

        //          int contentHeight = 0, offset = 0;

        //          contentHeight = Orientation == Orientation.Vertical
        //                              ? Children.Sum(c => c.Height) + (Children.Count - 1) * Spacing
        //                              : Children.Max(c => c.Height);

        //          if (VerticalContentAlignment == VerticalAlignment.FillParent)
        //          {
        //              if (Orientation == Orientation.Vertical)
        //              {
        //                  var contentLayoutHeight = (int)Math.Floor((Height - ((Children.Count - 1) * Spacing)) / (float)Children.Count);

        //                  ForEachChild(c =>
        //                  {
        //                      c.LayoutHeight  = contentLayoutHeight;
        //                      c.LayoutOffsetY = offset;

        //                      offset += contentLayoutHeight + Spacing;
        //                  });
        //              }
        //              else
        //              {
        //                  ForEachChild(c =>
        //                  {
        //                      c.LayoutHeight  = Height;
        //                      c.LayoutOffsetY = 0;
        //                  });
        //              }

        //              return;;
        //          }

        //          if (VerticalContentAlignment == VerticalAlignment.Top)
        //          {
        //              // Offset is 0
        //          }
        //          else if (VerticalContentAlignment == VerticalAlignment.Center)
        //          {
        //              offset = (int)((Height - contentHeight) / 2f);
        //          }
        //          else if (VerticalContentAlignment == VerticalAlignment.Bottom)
        //          {
        //              offset = Height - contentHeight;
        //          }


        //          ForEachChild(c =>
        //          {
        //              c.LayoutHeight = 0;
        //              c.LayoutOffsetY = offset;

        //              offset += c.Height + Spacing;
        //          });
        //      }

        //      protected void AlignContentHorizontally()
        //      {
        //          if (!HasChildren || HorizontalContentAlignment == HorizontalAlignment.None) return;

        //          int offset = 0;

        //          if (HorizontalContentAlignment == HorizontalAlignment.FillParent)
        //          {
        //              if (Orientation == Orientation.Horizontal)
        //              {
        //                  var contentLayoutWidth = (int)Math.Floor((Width - ((Children.Count - 1) * Spacing)) / (float)Children.Count);

        //                  ForEachChild(c =>
        //                  {
        //                      c.LayoutWidth  = contentLayoutWidth;
        //                      c.LayoutOffsetX = offset;

        //                      offset += contentLayoutWidth + Spacing;
        //                  });
        //              }
        //              else
        //              {
        //                  ForEachChild(c =>
        //                  {
        //                      c.LayoutWidth = Width;
        //                      c.LayoutOffsetX = 0;
        //                  });
        //              }

        //              return;
        //          }


        //	ForEachChild(c =>
        //	{
        //           if (HorizontalContentAlignment == HorizontalAlignment.Left)
        //           {
        //            c.LayoutOffsetX = 0;
        //           }
        //           else if (HorizontalContentAlignment == HorizontalAlignment.Center)
        //           {
        //            c.LayoutOffsetX = (int)((Width - c.Width) / 2f);
        //           }
        //           else if (HorizontalContentAlignment == HorizontalAlignment.Right)
        //           {
        //            c.LayoutOffsetX = (Width - (c.Width));
        //           }
        //          });
        //}
    }
}

