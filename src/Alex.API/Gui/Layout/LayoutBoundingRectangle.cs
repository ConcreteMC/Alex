using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.API.Gui.Layout
{
    public struct LayoutBoundingRectangle
    {
        public Rectangle ContainerBounds { get; }
        public Thickness ContainerPadding { get; }

        public Thickness Margin => Thickness.Max(ContainerPadding, ElementMargin);
        public Rectangle LayoutBounds => ContainerBounds - Margin;

        public Size ElementSize { get; }
        public Thickness ElementMargin { get; }

        public int? AnchorLeft;
        public int? AnchorTop;
        public int? AnchorRight;
        public int? AnchorBottom;

        public bool AnchorCenterX => !AnchorLeft.HasValue && !AnchorRight.HasValue;
        public bool AnchorCenterY => !AnchorTop.HasValue && !AnchorBottom.HasValue;

        public int Width => (AnchorLeft.HasValue && AnchorRight.HasValue)
                                ? LayoutBounds.Width - AnchorLeft.Value - AnchorRight.Value
                                : ElementSize.Width;
        public int Height => (AnchorTop.HasValue && AnchorBottom.HasValue)
                                ? LayoutBounds.Height - AnchorTop.Value - AnchorBottom.Value
                                : ElementSize.Height;

        public int AbsoluteX => AnchorCenterX
                                    ? LayoutBounds.X + (int) ((LayoutBounds.Width - Width) / 2f)
                                    : (AnchorRight.HasValue 
                                           ? LayoutBounds.X + (AnchorLeft ?? 0) + LayoutBounds.Width - Width 
                                           : LayoutBounds.X + (AnchorLeft ?? 0));

        public int AbsoluteY => AnchorCenterY
                                    ? LayoutBounds.Y + (int) ((LayoutBounds.Height - Height) / 2f)
                                    : (AnchorBottom.HasValue
                                           ? LayoutBounds.Y + (AnchorTop ?? 0) + LayoutBounds.Height - Height 
                                           : LayoutBounds.Y + (AnchorTop ?? 0));

        public int RelativeX => AbsoluteX - ContainerBounds.X;
        public int RelativeY => AbsoluteY - ContainerBounds.Y;
        public Point Offset => new Point(RelativeX, RelativeY);

        public Point Location => new Point(AbsoluteX, AbsoluteY);
        public Size Size => new Size(Width, Height);
        public Rectangle Bounds => new Rectangle(Location, Size);
        
        public LayoutBoundingRectangle(Rectangle containerBounds, Thickness containerPadding, Thickness elementMargin, Size elementSize)
        {
            ContainerBounds = containerBounds;
            ContainerPadding = containerPadding;
            ElementMargin = elementMargin;
            ElementSize = elementSize;

            AnchorLeft = null;
            AnchorTop = null;
            AnchorRight = null;
            AnchorBottom = null;
        }

        public Thickness ToThickness()
        {
            return new Thickness(AnchorLeft ?? 0, AnchorTop ?? 0, AnchorRight ?? 0, AnchorBottom ?? 0);
        }

        public void Anchor(int? left, int? top, int? right, int? bottom)
        {
            if (left.HasValue)
            {
                AnchorLeft += left;
            }

            if (top.HasValue)
            {
                AnchorTop += top;
            }

            if (right.HasValue)
            {
                AnchorRight += right;
            }

            if (bottom.HasValue)
            {
                AnchorBottom += bottom;
            }
        }
    }
}
