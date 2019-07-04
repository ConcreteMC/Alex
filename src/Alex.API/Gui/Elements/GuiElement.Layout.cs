using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Alex.API.Gui.Attributes;
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
        private Alignment    _anchor       = Alignment.Default;
        private AutoSizeMode _autoSizeMode = AutoSizeMode.GrowOnly;

        [Obsolete("Please use the Margin.Left property instead!")]
        [DebuggerVisible] public int X
        {
            set
            {
                var m = Margin;
                Margin = new Thickness(value, m.Top, m.Right, m.Bottom);
                InvalidateLayout();
            }
        }

        [Obsolete("Please use the Margin.Top property instead!")]
        [DebuggerVisible] public int Y
        {
            set
            {
                var m = Margin;
                Margin = new Thickness(m.Left, value, m.Right, m.Bottom);
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public int Width
        {
            get => _width;
            set
            {
                _width = value;
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public int Height
        {
            get => _height;
            set
            {
                _height = value;
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public int MinWidth
        {
            get => _minWidth;
            set
            {
                _minWidth = value;
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public int MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public int MinHeight
        {
            get => _minHeight;
            set
            {
                _minHeight = value;
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public int MaxHeight
        {
            get => _maxHeight;
            set
            {
                _maxHeight = value;
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public Thickness Padding
        {
            get => _padding;
            set
            {
                _padding = value;
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public Thickness Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public virtual AutoSizeMode AutoSizeMode
        {
            get => _autoSizeMode;
            set
            {
                _autoSizeMode = value;
                InvalidateLayout();
            }
        }

        [DebuggerVisible] public Alignment Anchor
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
        
        [DebuggerVisible] public bool IsSizeDirty { get; protected set; } = true;
        [DebuggerVisible] public bool IsLayoutDirty    { get; protected set; } = true;
        
        [DebuggerVisible] public bool IsMeasureComplete { get; protected set; } = false;
        [DebuggerVisible] public bool IsArrangeComplete { get; protected set; } = false;

        [DebuggerVisible] public int  LayoutOffsetX    { get; private set; }   = 0;
        [DebuggerVisible] public int  LayoutOffsetY    { get; private set; }   = 0;
        [DebuggerVisible] public int  LayoutWidth      { get; private set; }   = 0;
        [DebuggerVisible] public int  LayoutHeight     { get; private set; }   = 0;
        [DebuggerVisible] public Size PreferredSize    { get; private set; }
        [DebuggerVisible] public Size PreferredMinSize { get; private set; }
        [DebuggerVisible] public Size PreferredMaxSize { get; private set; }

        #endregion

        #region Calculated Properties

        [DebuggerVisible] public Rectangle Bounds      { get; private set; }
        [DebuggerVisible] public Rectangle InnerBounds { get; private set; }
        [DebuggerVisible] public Rectangle OuterBounds { get; private set; }
        [DebuggerVisible] public Point     Position    { get; private set; }
        [DebuggerVisible] public Size      Size        { get; private set; }
        [DebuggerVisible] public Size      ContentSize { get; private set; }

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
                OnBeforeMeasure();

                UpdatePreferredSize();

                var size = MeasureCore(availableSize - Margin);

                Size = size;

                OnAfterMeasure();

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

            size = Size.Clamp(size, PreferredMinSize, PreferredMaxSize);
            size = Size.Clamp(size, size, availableSize);

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

                if (Anchor.HasFlag(Alignment.FillX))
                {
                    size.Width = availableSize.Width;
                }

                if (Anchor.HasFlag(Alignment.FillY))
                {
                    size.Height = availableSize.Height;
                }
            }

            size = Size.Clamp(size, size, availableSize);

            var childSize = MeasureChildren(size);

            return Size.Max(size, childSize);
        }

        protected Size MeasureChildren(Size availableSize)
        {
            var children = Children.Cast<GuiElement>().ToArray();

            var size = MeasureChildrenCore(availableSize, children);

	        ContentSize = size;

			return size;
        }
        protected virtual Size MeasureChildrenCore(Size availableSize, IReadOnlyCollection<GuiElement> children)
        {
            Size size = Size.Zero;

            foreach (var child in children)
            {
                var childSize = child.Measure(availableSize);

                size = Size.Max(size, childSize);
            }

            if (size.Height > availableSize.Height)
            {
                size.Height = size.Height - availableSize.Height;
            }

            return size;
        }

        protected virtual void OnBeforeMeasure()
        {

        }
        protected virtual void OnAfterMeasure()
        {

        }

        #endregion

        #region Positioning Methods
        
        public void Arrange(Rectangle newBounds)
        {
            if (!IsArrangeComplete)
            {
                OnBeforeArrange(newBounds);

                OuterBounds = newBounds + Margin;
                InnerBounds = newBounds - Padding;
                Bounds      = newBounds;

                Size     = newBounds.Size;
                Position = newBounds.Location;
                
                IsLayoutDirty = false;

                ArrangeCore(newBounds);

                OnAfterArrange();

                IsArrangeComplete = true;
            }

        }

        protected virtual void OnBeforeArrange(Rectangle newBounds)
        {
            if (this.TryFindParent(e => e.ClipToBounds, out var parentClip))
            {
                RenderBounds = Rectangle.Intersect(newBounds, parentClip.RenderBounds);
                RenderSize = newBounds.Size;
                RenderPosition = newBounds.Location.ToVector2();
            }
            else
            {
                RenderBounds = newBounds;
                RenderSize = newBounds.Size;
                RenderPosition = newBounds.Location.ToVector2();
            }
        }

        protected virtual void OnAfterArrange()
        {
            //if (ParentElement != null && ClipToBounds)
            //{
            //    RenderBounds   = Rectangle.Intersect(Bounds, ParentElement.RenderBounds);
            //    RenderSize     = RenderBounds.Size;
            //    RenderPosition = RenderBounds.Location.ToVector2();
            //}
            //else
            //{
            //    RenderBounds   = Bounds;
            //    RenderSize     = Size;
            //    RenderPosition = Position.ToVector2();
            //}
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
                if(ShouldPositionChild(child))
                    PositionChild(child, child.Anchor, newBounds, Padding, Thickness.Zero);
            }
        }

        protected virtual bool ShouldPositionChild(GuiElement child)
        {
            return true;
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

            child.IsLayoutDirty = false;

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