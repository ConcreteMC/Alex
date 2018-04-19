using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Layout;
using Alex.API.Gui.Rendering;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui
{
    public delegate bool GuiElementPredicate(IGuiElement element);
    public delegate bool GuiElementPredicate<in TGuiElement>(TGuiElement element) where TGuiElement : class, IGuiElement;

    public class GuiElement : IGuiElement
    {
        private IGuiScreen _screen;
        private IGuiElement _parentElement;

        public IGuiScreen Screen
        {
            get => _screen;
            private set
            {
                var currentScreen = _screen;
                _screen = value;
                OnScreenChanged(currentScreen, _screen);
                InvalidateLayout();
            }
        }
        public IGuiElement ParentElement
        {
            get => _parentElement;
            set
            {
                _parentElement = value;
                TryFindParentOfType<IGuiScreen>(e => true, out IGuiScreen screen);
                Screen = screen;
                OnParentElementChanged(_parentElement);
                InvalidateLayout();
            }
        }
        
        protected List<IGuiElement> Children { get; } = new List<IGuiElement>();
        public bool HasChildren => Children.Any();
        
        #region Drawing

        private float _rotation;

        public float Rotation
        {
            get => _rotation;
            set => _rotation = MathHelper.ToRadians(value);
        }

        public virtual Vector2 RotationOrigin { get; set; } = Vector2.Zero;
        
        private Color? _backgroundOverlayColor;

        public GuiTextures? DefaultBackgroundTexture { get; set; }
        public TextureRepeatMode BackgroundRepeatMode { get; set; } = TextureRepeatMode.Stretch;
        public TextureSlice2D DefaultBackground { get; set; }
        public TextureSlice2D Background { get; set; }
        public Vector2 BackgroundScale { get; set; } = Vector2.One;
        
        public Color? BackgroundOverlayColor
        {
            get => _backgroundOverlayColor;
            set
            {
                _backgroundOverlayColor = value;
                BackgroundOverlay = null;
            }
        }

        public TextureSlice2D BackgroundOverlay { get; set; }
        
        public virtual Vector2 RenderPosition => Position.ToVector2();
        public virtual Size RenderSize => Size;
        public virtual Rectangle RenderBounds => Bounds;
        
        public void Draw(GuiRenderArgs renderArgs)
        {
            // Init matrix
            OnDraw(renderArgs);

            ForEachChild(c => c.Draw(renderArgs));
        }

        protected virtual void OnDraw(GuiRenderArgs args)
        {
            if (BackgroundOverlayColor.HasValue && BackgroundOverlay == null)
            {
                BackgroundOverlay = new ColorTexture2D(args.Graphics, BackgroundOverlayColor.Value);
            }

            if (Background != null)
            {
                args.Draw(Background, RenderBounds, BackgroundRepeatMode, BackgroundScale);
            }

            if (BackgroundOverlay != null)
            {
                args.Draw(BackgroundOverlay, RenderBounds, BackgroundRepeatMode, BackgroundScale);
            }
        }
        
        #endregion

        private IGuiRenderer _guiRenderer;
        private bool _initialised;

        protected IGuiRenderer GuiRenderer => _guiRenderer;
        
        public GuiElement()
        {

        }

        #region Methods
        
        public void Init(IGuiRenderer renderer)
        {
            if (!_initialised)
            {
                OnInit(renderer);
                _guiRenderer = renderer;
            }

            ForEachChild(c => c.Init(renderer));

            _initialised = true;
        }
        protected virtual void OnInit(IGuiRenderer renderer)
        {
            if (DefaultBackgroundTexture.HasValue)
            {
                DefaultBackground = renderer.GetTexture(DefaultBackgroundTexture.Value);
            }

            if (Background == null && DefaultBackground != null)
            {
                Background = DefaultBackground;
            }
        }

        public void Update(GameTime gameTime)
        {
            OnUpdate(gameTime);

            ForEachChild(c => c.Update(gameTime));
        }
        protected virtual void OnUpdate(GameTime gameTime)
        {

        }
        

        public void AddChild(IGuiElement element)
        {
            if (element == this) return;

            element.ParentElement = this;
            Children.Add(element);
            if (_initialised)
            {
                element.Init(_guiRenderer);
            }
            OnChildAdded(element);
            InvalidateLayout();
        }

        public void RemoveChild(IGuiElement element)
        {
            if (element == this) return;

            OnChildRemoved(element);

            Children.Remove(element);
            element.ParentElement = null;
            InvalidateLayout();
        }
        
        #endregion


        #region Hierachy Transcending

        public bool TryTranscendChildren(GuiElementPredicate predicate, bool recurse = true)
        {
            if (!HasChildren) return false;

            var children = Children.ToArray();

            // First scan the children at this level
            foreach (var child in children)
            {
                if (predicate(child))
                {
                    return true;
                }
            }

            if (!recurse) return false;

            // If the children on this level do not match, check their children.
            foreach (var child in children)
            {
                if (child.TryTranscendChildren(predicate, true))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryFindParent(GuiElementPredicate predicate, out IGuiElement parentElement)
        {
            if (ParentElement == null)
            {
                parentElement = null;
                return false;
            }

            if (predicate(ParentElement))
            {
                parentElement = ParentElement;
                return true;
            }

            return ParentElement.TryFindParent(predicate, out parentElement);
        }

        public bool TryFindParentOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement parentElement) where TGuiElement : class, IGuiElement
        {

            var result = TryFindParent(e => e is TGuiElement e1 && predicate(e1), out IGuiElement element);

            parentElement = element as TGuiElement;
            return result;
        }

        public bool TryFindDeepestChild(GuiElementPredicate predicate, out IGuiElement childElement)
        {
            childElement = null;
            if (!HasChildren) return false;

            var children = Children.ToArray();

            foreach (var child in children)
            {
                if (predicate(child))
                {
                    childElement = child;

                    if (child.TryFindDeepestChild(predicate, out var recurseChild))
                    {
                        childElement = recurseChild;
                        return true;
                    }

                    return true;
                }
            }

            // If the children on this level do not match, check their children.
            foreach (var child in children)
            {
                if (child.TryFindDeepestChild(predicate, out var recurseChild))
                {
                    childElement = recurseChild;
                    return true;
                }
            }

            return false;
        }

        public bool TryFindDeepestChildOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement childElement) where TGuiElement : class, IGuiElement
        {
            var result = TryFindDeepestChild(e => e is TGuiElement e1 && predicate(e1), out IGuiElement element);

            childElement = element as TGuiElement;
            return result;
        }

        public IEnumerable<TResult> ForEachChild<TResult>(Func<IGuiElement, TResult> valueSelector)
        {
            if (HasChildren)
            {
                foreach (var child in Children.ToArray())
                {
                    yield return valueSelector(child);
                }
            }
        }

        public void ForEachChild(Action<IGuiElement> childAction)
        {
            if (!HasChildren) return;

            foreach (var child in Children.ToArray())
            {
                childAction(child);
            }
        }

        private void ForEachChild<TElement>(Action<TElement> childAction) where TElement : class, IGuiElement
        {
            ForEachChild(c =>
            {
                if(c is TElement e) childAction(e);
            });
        }

        #endregion

        #region Event Handlers
        
        protected virtual void OnChildAdded(IGuiElement element) { }

        protected virtual void OnChildRemoved(IGuiElement element) { }

        protected virtual void OnScreenChanged(IGuiScreen previousScreen, IGuiScreen newScreen) { }

        protected virtual void OnParentElementChanged(IGuiElement parentElement) { }
        
        protected virtual void OnUpdateLayout() { }

        #endregion

        #region Layout Engine

        #region Properties
        
        private int _width = 0;
        private int _height = 0;
        private int _minWidth  = 0;
        private int _minHeight = 0;
        private int _maxWidth  = int.MaxValue;
        private int _maxHeight = int.MaxValue;
        private Thickness _padding = Thickness.Zero;
        private Thickness _margin = Thickness.Zero;
        private AutoSizeMode _autoSizeMode = AutoSizeMode.GrowOnly;
        private Alignment _anchor = Alignment.Default;
        
        [Obsolete("Please use the Margin.Left property instead!")]
        public int X
        {
            set
            {
                var m = Margin;
                Margin  = new Thickness(value, m.Top, m.Right, m.Bottom);
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
        
        public bool IsLayoutDirty      { get; protected set; } = true;
        
        public int LayoutOffsetX { get; private set; } = 0;
        public int LayoutOffsetY { get; private set; } = 0;
        public int LayoutWidth   { get; private set; } = 0;
        public int LayoutHeight  { get; private set; } = 0;

        public Size PreferredSize { get; private set; }
        public Size PreferredMinSize { get; private set; }
        public Size PreferredMaxSize { get; private set; }

        #endregion

        #region Calculated Properties

        public Rectangle Bounds { get; private set; }
        public Rectangle InnerBounds { get; private set; }
        public Rectangle OuterBounds { get; private set; }

        public Point Position { get; private set; }
        public Size Size { get; private set; }

        #endregion

        #region Methods

        protected void SetFixedSize(int width, int height)
        {
            AutoSizeMode = AutoSizeMode.None;
            MinWidth = width;
            MaxWidth = width;
            MinHeight = height;
            MaxHeight = height;
            Width = width;
            Height = height;
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

        #region Sizing
        
        protected void UpdatePreferredSize()
        {
            GetPreferredSize(out var size, out var minSize, out var maxSize);

            PreferredSize    = size;
            PreferredMinSize = minSize;
            PreferredMaxSize = maxSize;
        }
        

        #endregion

        #region Positioning

        

        #endregion


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
                PositionChild(child, child.Anchor, newBounds, Thickness.Zero);
            }
        }
        
        protected LayoutBoundingRectangle PositionChild(GuiElement child, Alignment alignment, Rectangle availableBounds, Thickness offset, bool forceAlignment = false)
        {
            var bounds = new LayoutBoundingRectangle(availableBounds, child.Margin, child.Size);
            
            PositionChildCore(child, ref bounds, offset, forceAlignment ? alignment : (child.Anchor & alignment));

            child.Arrange(bounds.Bounds);
            return bounds;
        }

        protected virtual void PositionChildCore(GuiElement child, ref LayoutBoundingRectangle bounds, Thickness offset, Alignment alignment)
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
            
            //if (vertical != Alignment.NoneY && vertical != Alignment.None)
            {
                child.LayoutOffsetY = bounds.RelativeY;
                child.LayoutHeight  = bounds.Height;
            }

            //if (horizontal != Alignment.NoneX && horizontal != Alignment.None)
            {
                child.LayoutOffsetX = bounds.RelativeX;
                child.LayoutWidth   = bounds.Width;
            }
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

            LayoutWidth = size.Height;
            LayoutHeight = size.Width;

            return size;
        }

        protected virtual void GetPreferredSize(out Size size, out Size minSize, out Size maxSize)
        {
            size = new Size(Width, Height);
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

#endregion
    }
}

