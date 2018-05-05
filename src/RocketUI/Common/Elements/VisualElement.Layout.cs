using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using RocketUI.Annotations;
using RocketUI.Styling;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RocketUI.Elements
{
    public partial class VisualElement : INotifyPropertyChanged
    {
        #region StyledProperties

        public event StyledPropertyChangedEventHandler WidthChanged;
        public event StyledPropertyChangedEventHandler HeightChanged;
        public event StyledPropertyChangedEventHandler MinWidthChanged;
        public event StyledPropertyChangedEventHandler MaxWidthChanged;
        public event StyledPropertyChangedEventHandler MinHeightChanged;
        public event StyledPropertyChangedEventHandler MaxHeightChanged;
        public event StyledPropertyChangedEventHandler PaddingChanged;
        public event StyledPropertyChangedEventHandler MarginChanged;
        public event StyledPropertyChangedEventHandler AutoSizeModeChanged;
        public event StyledPropertyChangedEventHandler AnchorChanged;

        //private static StyledProperty WidthProperty     = StyledProperty.Register(nameof(Width),     typeof(VisualElement), typeof(int));
        //private static StyledProperty HeightProperty    = StyledProperty.Register(nameof(Height),    typeof(VisualElement), typeof(int));
        //private static StyledProperty MinWidthProperty  = StyledProperty.Register(nameof(MinWidth),  typeof(VisualElement), typeof(int));
        //private static StyledProperty MinHeightProperty = StyledProperty.Register(nameof(MinHeight), typeof(VisualElement), typeof(int));
        //private static StyledProperty MaxWidthProperty  = StyledProperty.Register(nameof(MaxWidth),  typeof(VisualElement), typeof(int));
        //private static StyledProperty MaxHeightProperty = StyledProperty.Register(nameof(MaxHeight), typeof(VisualElement), typeof(int));
        //private static StyledProperty                   PaddingProperty = StyledProperty.Register(nameof(MaxHeight), typeof(VisualElement), typeof(int));
        
        #endregion


        #region Properties
        
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

        [StyledProperty]
        public int Width
        {
            get => (int) GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        [StyledProperty]
        public int Height
        {
            get => (int)GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        [StyledProperty]
        public int MinWidth
        {
            get => (int)GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        [StyledProperty]
        public int MaxWidth
        {
            get => (int)GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        [StyledProperty]
        public int MinHeight
        {
            get => (int)GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        [StyledProperty]
        public int MaxHeight
        {
            get => (int)GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        [StyledProperty]
        public Thickness Padding
        {
            get => (Thickness)GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        [StyledProperty]
        public Thickness Margin
        {
            get => (Thickness)GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        [StyledProperty]
        public AutoSizeMode AutoSizeMode
        {
            get => (AutoSizeMode)GetValue();
            set => SetValue(value, InvalidateLayout);
        }
        [StyledProperty]
        public Anchor Anchor
        {
            get => (Anchor)GetValue();
            set => SetValue(value, InvalidateLayout);
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

        public void InvalidateLayout()
        {
            InvalidateLayout(this);
        }

        public void InvalidateLayout(IVisualElement sender)
        {
            //if ((ParentElement as VisualElement)?.IsLayoutInProgress ?? false)
            //{
            //    return;
            //}

            IsSizeDirty = true;
            IsLayoutDirty = true;

            ForEachChild(c =>
            {
                if (c != sender)
                {
                    c.InvalidateLayout(this, true);
                }
            });
            
            if (ParentElement != sender)
            {
                ParentElement?.InvalidateLayout(this);
            }
        }

        protected internal void DoLayoutSizing()
        {
            if (IsSizeDirty)
            {
                UpdatePreferredSize();
            }

            ForEachChild<VisualElement>(c => c.DoLayoutSizing());
        }

        protected internal void BeginLayoutMeasure()
        {
            IsMeasureComplete = false;

            ForEachChild<VisualElement>(c => c.BeginLayoutMeasure());
        }

        protected internal void BeginLayoutArrange()
        {
            IsArrangeComplete = false;

            ForEachChild<VisualElement>(c => c.BeginLayoutArrange());
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
            var children = Children.Cast<VisualElement>().ToArray();

            var size = MeasureChildrenCore(availableSize - Padding, children);

            return size + Padding;
        }
        protected virtual Size MeasureChildrenCore(Size availableSize, IReadOnlyCollection<VisualElement> children)
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
            var children = new ReadOnlyCollection<VisualElement>(Children.Cast<VisualElement>().ToList());

            ArrangeChildrenCore(newBounds, children);
        }

        protected virtual void ArrangeChildrenCore(Rectangle newBounds, IReadOnlyCollection<VisualElement> children)
        {
            foreach (var child in children)
            {
                PositionChild(child, child.Anchor, newBounds, Padding, Thickness.Zero);
            }
        }

        protected LayoutBoundingRectangle PositionChild(VisualElement child,           Anchor anchor,
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

        protected virtual void PositionChildCore(VisualElement child, ref LayoutBoundingRectangle bounds, Thickness offset, Anchor  anchor)
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


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}