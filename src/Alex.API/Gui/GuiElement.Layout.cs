using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Alex.API.Gui.Layout;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui
{
    public partial class GuiElement
    {

        #region Properties

        private int          _width        = 0;
        private int          _height       = 0;
        private int          _minWidth     = 0;
        private int          _minHeight    = 0;
        private int          _maxWidth     = int.MaxValue;
        private int          _maxHeight    = int.MaxValue;
        private Thickness    _margin       = Thickness.Zero;
        private Thickness    _padding      = Thickness.Zero;
        private Alignment    _anchor       = Alignment.Default;
        private AutoSizeMode _autoSizeMode = AutoSizeMode.GrowOnly;

        [Obsolete("Please use the Margin.Left property instead!")]
        public int X
        {
            set
            {
                var m = Margin;
                Margin = new Thickness(value, m.Top, m.Right, m.Bottom);
                InvalidateLayout();
            }
        }

        [Obsolete("Please use the Margin.Top property instead!")]
        public int Y
        {
            set
            {
                var m = Margin;
                Margin = new Thickness(m.Left, value, m.Right, m.Bottom);
                InvalidateLayout();
            }
        }

        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                InvalidateLayout();
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                InvalidateLayout();
            }
        }

        public int MinWidth
        {
            get => _minWidth;
            set
            {
                _minWidth = value;
                InvalidateLayout();
            }
        }

        public int MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                InvalidateLayout();
            }
        }

        public int MinHeight
        {
            get => _minHeight;
            set
            {
                _minHeight = value;
                InvalidateLayout();
            }
        }

        public int MaxHeight
        {
            get => _maxHeight;
            set
            {
                _maxHeight = value;
                InvalidateLayout();
            }
        }

        public Thickness Padding
        {
            get => _padding;
            set
            {
                _padding = value;
                InvalidateLayout();
            }
        }

        public Thickness Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                InvalidateLayout();
            }
        }

        public virtual AutoSizeMode AutoSizeMode
        {
            get => _autoSizeMode;
            set
            {
                _autoSizeMode = value;
                InvalidateLayout();
            }
        }

        public Alignment Anchor
        {
            get => _anchor;
            set
            {
                _anchor = value;
                InvalidateLayout();
            }
        }

        #endregion

        #region Layout Calculation State Properties

        public bool IsLayoutDirty    { get; protected set; } = true;
        public int  LayoutOffsetX    { get; private set; }   = 0;
        public int  LayoutOffsetY    { get; private set; }   = 0;
        public int  LayoutWidth      { get; private set; }   = 0;
        public int  LayoutHeight     { get; private set; }   = 0;
        public Size PreferredSize    { get; private set; }
        public Size PreferredMinSize { get; private set; }
        public Size PreferredMaxSize { get; private set; }

        #endregion

        #region Calculated Properties

        public Rectangle Bounds      { get; private set; }
        public Rectangle InnerBounds { get; private set; }
        public Rectangle OuterBounds { get; private set; }
        public Point     Position    { get; private set; }
        public Size      Size        { get; private set; }

        #endregion


        protected void SetFixedSize(int width, int height)
        {
            AutoSizeMode = AutoSizeMode.None;
            MinWidth     = width;
            MaxWidth     = width;
            MinHeight    = height;
            MaxHeight    = height;
            Width        = width;
            Height       = height;
        }

        public void InvalidateLayout(bool invalidateChildren = true)
        {
            InvalidateLayout(this, invalidateChildren);
        }

        public void InvalidateLayout(IGuiElement sender, bool invalidateChildren = true)
        {
            //if ((ParentElement as GuiElement)?.IsLayoutInProgress ?? false)
            //{
            //    return;
            //}

            IsLayoutDirty = true;

            if (invalidateChildren)
            {
                ForEachChild(c =>
                {
                    if (c != sender)
                    {
                        c.InvalidateLayout(this, true);
                    }
                });
            }

            if (ParentElement != sender)
            {
                ParentElement?.InvalidateLayout(this, invalidateChildren);
            }
        }

        protected internal void DoLayoutSizing()
        {
            UpdatePreferredSize();

            ForEachChild<GuiElement>(c => c.DoLayoutSizing());
        }
        
        protected internal void DoLayoutMeasure(Size parentSize)
        {
            var size = Measure(parentSize);

            ForEachChild<GuiElement>(c => c.DoLayoutMeasure(size));
        }
        protected internal void DoLayoutArrange(Point parentPosition)
        {
            if (IsLayoutDirty)
            {
                //Arrange(new Rectangle(parentPosition.X + LayoutOffsetX, parentPosition.Y + LayoutOffsetY, LayoutWidth, LayoutHeight));
                //Arrange(Bounds);
            }

            //ForEachChild<GuiElement>(c => c.DoLayoutArrange(Position));
        }
        
        #region Sizing Methods

        protected void UpdatePreferredSize()
        {
            GetPreferredSize(out var size, out var minSize, out var maxSize);

            PreferredSize    = size;
            PreferredMinSize = minSize;
            PreferredMaxSize = maxSize;
        }
        
        public Size Measure(Size availableSize)
        {
            UpdatePreferredSize();

            var size = MeasureCore(availableSize);

            Size = size;

            return Size + Margin;
        }

        protected virtual Size MeasureCore(Size availableSize)
        {
            var size = PreferredSize;

            if (AutoSizeMode == AutoSizeMode.None)
            {
                // AS DEFINED ONLY.
                // Make sure we still measure all children though!
                MeasureChildren(size);
            }
            else
            {
                var autoSize = MeasureAutoSize(availableSize);

                if (Anchor.HasFlag(Alignment.FillX))
                {
                    autoSize.Width = Math.Max(availableSize.Width, autoSize.Width);
                }

                if (Anchor.HasFlag(Alignment.FillY))
                {
                    autoSize.Height = Math.Max(availableSize.Height, autoSize.Height);
                }

                size = autoSize;
            }

            LayoutWidth  = size.Height;
            LayoutHeight = size.Width;

            return size;
        }

        protected virtual void GetPreferredSize(out Size size, out Size minSize, out Size maxSize)
        {
            size    = new Size(Width, Height);
            minSize = new Size(MinWidth, MinHeight);
            maxSize = new Size(MaxWidth, MaxHeight);

            if (AutoSizeMode == AutoSizeMode.None)
            {
                minSize = size;
                maxSize = size;
            }
            else if (AutoSizeMode == AutoSizeMode.GrowOnly)
            {
                minSize = size;
            }

            size = Size.Clamp(size, minSize, maxSize);
        }

        private Size MeasureAutoSize(Size availableSize)
        {
            var size = PreferredMinSize;

            if (AutoSizeMode == AutoSizeMode.None)
            {
                size = PreferredSize;
            }
            else
            {
                if (AutoSizeMode == AutoSizeMode.GrowOnly)
                {
                    size = Size.Clamp(size, PreferredSize, PreferredMaxSize);
                }

                if (AutoSizeMode == AutoSizeMode.GrowAndShrink)
                {
                    size = Size.Min(size, availableSize);
                    size = Size.Clamp(size, PreferredMinSize, availableSize);
                }

                if (Anchor.HasFlag(Alignment.FillX))
                {
                    size.Width = availableSize.Width;
                }

                if (Anchor.HasFlag(Alignment.FillY))
                {
                    size.Height = availableSize.Height;
                }
            }

            var childSize = MeasureChildren(size);

            return Size.Max(size, childSize);
        }

        protected Size MeasureChildren(Size availableSize)
        {
            var children = Children.Cast<GuiElement>().ToArray();

            var size = MeasureChildrenCore(availableSize - Padding, children);

            return size + Padding;
        }

        protected virtual Size MeasureChildrenCore(Size availableSize, IReadOnlyCollection<GuiElement> children)
        {
            Size size = Size.Zero;

            foreach (var child in children)
            {
                var childSize = child.Measure(availableSize);

                size = Size.Max(size, childSize);
            }

            return size;
        }

        #endregion

        #region Positioning Methods
        
        public void Arrange(Rectangle newBounds)
        {
            OuterBounds = newBounds + Margin;
            InnerBounds = newBounds - Padding;
            Bounds      = newBounds;

            Size     = newBounds.Size;
            Position = newBounds.Location;

            ArrangeCore(newBounds);

            IsLayoutDirty = false;
        }

        protected virtual void ArrangeCore(Rectangle newBounds)
        {
            ArrangeChildren(newBounds - Padding);
        }

        protected void ArrangeChildren(Rectangle newBounds)
        {
            var children = new ReadOnlyCollection<GuiElement>(Children.Cast<GuiElement>().ToList());

            ArrangeChildrenCore(newBounds, children);
        }

        protected virtual void ArrangeChildrenCore(Rectangle newBounds, IReadOnlyCollection<GuiElement> children)
        {
            foreach (var child in children)
            {
                PositionChild(child, child.Anchor, newBounds, Padding, Thickness.Zero);
            }
        }

        protected LayoutBoundingRectangle PositionChild(GuiElement child,           Alignment alignment,
                                                        Rectangle  availableBounds, Thickness padding, Thickness offset,
                                                        bool       forceAlignment = false)
        {
            var bounds = new LayoutBoundingRectangle(availableBounds, padding, child.Margin, child.Size);

            PositionChildCore(child, ref bounds, offset, forceAlignment ? alignment : (child.Anchor & alignment));
            
            child.LayoutOffsetY = bounds.RelativeY;
            child.LayoutHeight  = bounds.Height;
            child.LayoutOffsetX = bounds.RelativeX;
            child.LayoutWidth   = bounds.Width;

            child.Arrange(bounds.Bounds);

            return bounds;
        }

        protected virtual void PositionChildCore(GuiElement child, ref LayoutBoundingRectangle bounds, Thickness offset, Alignment  alignment)
        {
            // Arrange Y
            var vertical = (alignment & (Alignment.OrientationY));

            if (vertical == Alignment.CenterY)
            {
                bounds.AnchorTop    = null;
                bounds.AnchorBottom = null;
            }

            if (vertical.HasFlag(Alignment.MinY))
            {
                bounds.AnchorTop = offset.Top;
            }

            if (vertical.HasFlag(Alignment.MaxY))
            {
                bounds.AnchorBottom = offset.Bottom;
            }


            var horizontal = (alignment & (Alignment.OrientationX));

            if (horizontal == Alignment.CenterX)
            {
                bounds.AnchorLeft  = null;
                bounds.AnchorRight = null;
            }

            if (horizontal.HasFlag(Alignment.MinX))
            {
                bounds.AnchorLeft = offset.Left;
            }

            if (horizontal.HasFlag(Alignment.MaxX))
            {
                bounds.AnchorRight = offset.Right;
            }
        }
        
        #endregion
        

    }
}