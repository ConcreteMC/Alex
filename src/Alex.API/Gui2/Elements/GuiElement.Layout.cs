using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Alex.API.Gui.Layout;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements
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
        private Anchor    _anchor       = Gui.Anchor.Default;
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

        public Anchor Anchor
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
        
        public bool IsSizeDirty { get; protected set; } = true;
        public bool IsLayoutDirty    { get; protected set; } = true;
        
        public bool IsMeasureComplete { get; protected set; } = false;
        public bool IsArrangeComplete { get; protected set; } = false;

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
	    public Size      ContentSize { get; private set; }

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

            IsSizeDirty = true;
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
            if (IsSizeDirty)
            {
                UpdatePreferredSize();
            }

            ForEachChild<GuiElement>(c => c.DoLayoutSizing());
        }

        protected internal void BeginLayoutMeasure()
        {
            IsMeasureComplete = false;

            ForEachChild<GuiElement>(c => c.BeginLayoutMeasure());
        }

        protected internal void BeginLayoutArrange()
        {
            IsArrangeComplete = false;

            ForEachChild<GuiElement>(c => c.BeginLayoutArrange());
        }
        
        #region Sizing Methods

        protected void UpdatePreferredSize()
        {
            GetPreferredSize(out var size, out var minSize, out var maxSize);

            PreferredSize    = size;
            PreferredMinSize = minSize;
            PreferredMaxSize = maxSize;
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
        
        public Size Measure(Size availableSize)
        {
            if (!IsMeasureComplete)
            {
                UpdatePreferredSize();

                var size = MeasureCore(availableSize - Margin);

                Size = size;

                IsMeasureComplete = true;
            }

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

                if (Anchor.HasFlag(Anchor.FillX))
                {
                    autoSize.Width = Math.Max(availableSize.Width, autoSize.Width);
                }

                if (Anchor.HasFlag(Anchor.FillY))
                {
                    autoSize.Height = Math.Max(availableSize.Height, autoSize.Height);
                }

                size = autoSize;
            }

            size = Size.Clamp(size, PreferredMinSize, PreferredMaxSize);

            LayoutWidth  = size.Height;
            LayoutHeight = size.Width;

            return size;
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

                if (Anchor.HasFlag(Anchor.FillX))
                {
                    size.Width = availableSize.Width;
                }

                if (Anchor.HasFlag(Anchor.FillY))
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

	        ContentSize = size;

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
            if (!IsArrangeComplete)
            {
                OuterBounds = newBounds + Margin;
                InnerBounds = newBounds - Padding;
                Bounds      = newBounds;

                Size     = newBounds.Size;
                Position = newBounds.Location;
                
                IsLayoutDirty = false;

                ArrangeCore(newBounds);

                IsArrangeComplete = true;
            }

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

        protected LayoutBoundingRectangle PositionChild(GuiElement child,           Anchor anchor,
                                                        Rectangle  availableBounds, Thickness padding, Thickness offset,
                                                        bool       forceAlignment = false)
        {
            var bounds = new LayoutBoundingRectangle(availableBounds, padding, child.Margin, child.Size);

            PositionChildCore(child, ref bounds, offset, forceAlignment ? anchor : (child.Anchor & anchor));
            
            child.LayoutOffsetY = bounds.RelativeY;
            child.LayoutHeight  = bounds.Height;
            child.LayoutOffsetX = bounds.RelativeX;
            child.LayoutWidth   = bounds.Width;

            child.IsLayoutDirty = false;

            child.Arrange(bounds.Bounds);

            return bounds;
        }

        protected virtual void PositionChildCore(GuiElement child, ref LayoutBoundingRectangle bounds, Thickness offset, Anchor  anchor)
        {
            // Arrange Y
            var vertical = (anchor & (Anchor.OrientationY));

            if (vertical == Anchor.CenterY)
            {
                bounds.AnchorTop    = null;
                bounds.AnchorBottom = null;
            }

            if (vertical.HasFlag(Anchor.MinY))
            {
                bounds.AnchorTop = offset.Top;
            }
            if (vertical.HasFlag(Anchor.MaxY))
            {
                bounds.AnchorBottom = offset.Bottom;
            }


            var horizontal = (anchor & (Anchor.OrientationX));

            if (horizontal == Anchor.CenterX)
            {
                bounds.AnchorLeft  = null;
                bounds.AnchorRight = null;
            }

            if (horizontal.HasFlag(Anchor.MinX))
            {
                bounds.AnchorLeft = offset.Left;
            }

            if (horizontal.HasFlag(Anchor.MaxX))
            {
                bounds.AnchorRight = offset.Right;
            }
        }
        
        #endregion
        

    }
}