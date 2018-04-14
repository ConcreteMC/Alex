using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui
{
    public delegate bool GuiElementPredicate(GuiElement element);

    public class GuiElement
    {
        public GuiElement ParentElement { get; set; }
        
        public virtual int X  { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

        public virtual int LayoutOffsetX { get; set; } = 0;
        public virtual int LayoutOffsetY { get; set; } = 0;
        public virtual int LayoutWidth { get; set; } = -1;
        public virtual int LayoutHeight { get; set; } = -1;

        protected List<GuiElement> Children { get; } = new List<GuiElement>();
        public bool HasChildren => Children.Any();

        protected internal IEnumerable<GuiElement> AllChildElements => Children.SelectMany(c => c.AllChildElements);

        protected Color DebugColor { get; set; } = Color.Red;

        #region Layout

        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.None;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.None;

        #endregion

        #region Drawing
        
        public float Rotation { get; set; }

        public virtual Vector2 RotationOrigin { get; set; } = Vector2.Zero;

        public GuiTextures? DefaultBackgroundTexture { get; set; }
        public TextureRepeatMode BackgroundRepeatMode { get; set; } = TextureRepeatMode.Stretch;
        public TextureSlice2D DefaultBackground { get; set; }
        public TextureSlice2D Background { get; set; }
        public Vector2 BackgroundScale { get; set; } = Vector2.One;

        private Color? _backgroundOverlayColor;

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

        private int _width = -1;
        private int _height = -1;

        public virtual int Width
        {
            get
            {
                if (ParentElement != null && HorizontalAlignment == HorizontalAlignment.Stretch) return ParentElement.Width;
                if (_width < 0)
                {
                    var childrenToCheck = Children.Where(c => c.HorizontalAlignment != HorizontalAlignment.Stretch).ToArray();
                    return (childrenToCheck.Any()
                                ? Math.Abs(childrenToCheck.Max(c => c.Bounds.Right) - childrenToCheck.Min(c => c.Bounds.Left))
                                : 0);
                }

                return _width;
            }

            set => _width = value;
        }

        public virtual int Height
        {
            get
            {
                if (ParentElement != null && VerticalAlignment == VerticalAlignment.Stretch) return ParentElement.Height;
                if (_height < 0)
                {
                    var childrenToCheck = Children.Where(c => c.VerticalAlignment != VerticalAlignment.Stretch).ToArray();
                    return (childrenToCheck.Any()
                                ? Math.Abs(childrenToCheck.Max(c => c.Bounds.Bottom) - childrenToCheck.Min(c => c.Bounds.Top))
                                : 0);
                }

                return _height;
            }

            set => _height = value;
        }

        public virtual Vector2   Position => (ParentElement?.Position ?? Vector2.Zero) + new Vector2(LayoutOffsetX, LayoutOffsetY) + new Vector2(X, Y);

        public virtual Point Size => new Point(LayoutWidth > 0 ? LayoutWidth : Width, LayoutHeight > 0 ? LayoutHeight : Height);
        public         Rectangle Bounds   => new Rectangle(Position.ToPoint(), Size);

        #endregion

        private IGuiRenderer _guiRenderer;
        private bool _initialised;

        public GuiElement()
        {

        }

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

        public void UpdateLayout()
        {
            OnUpdateLayout();

            ForEachChild(c => c.UpdateLayout());
        }

        protected virtual void OnUpdateLayout()
        {
            AlignVertically();
            AlignHorizontally();
        }

        protected void AlignVertically()
        {
            if (ParentElement == null || VerticalAlignment == VerticalAlignment.None) return;
            if (VerticalAlignment == VerticalAlignment.Top)
            {
                LayoutOffsetY = 0;
            }
            else if (VerticalAlignment == VerticalAlignment.Center)
            {
                LayoutOffsetY = (int)((ParentElement.Size.Y - Size.Y) / 2f);
            }
            else if (VerticalAlignment == VerticalAlignment.Bottom)
            {
                LayoutOffsetY = (int) (ParentElement.Size.Y - Size.Y);
            }
        }
        protected void AlignHorizontally()
        {
            if (ParentElement == null || HorizontalAlignment == HorizontalAlignment.None) return;
            if (HorizontalAlignment == HorizontalAlignment.Left)
            {
                LayoutOffsetX = 0;
            }
            else if (HorizontalAlignment == HorizontalAlignment.Center)
            {
                LayoutOffsetX = (int)((ParentElement.Size.X - Size.X) / 2f);
            }
            else if (HorizontalAlignment == HorizontalAlignment.Right)
            {
                LayoutOffsetX = (int) (ParentElement.Size.X - Size.X);
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


        public void Draw(GuiRenderArgs renderArgs)
        {
            // Init matrix
            OnDraw(renderArgs);

            ForEachChild(c => c.Draw(renderArgs));
        }

        protected virtual void OnDraw(GuiRenderArgs args)
        {
            args.DrawRectangle(Bounds, DebugColor);

            if (BackgroundOverlayColor.HasValue && BackgroundOverlay == null)
            {
                BackgroundOverlay = new ColorTexture2D(args.Graphics, BackgroundOverlayColor.Value);
            }

            if (Background != null)
            {
                args.Draw(Background, Bounds, BackgroundRepeatMode, BackgroundScale);
            }

            if (BackgroundOverlay != null)
            {
                args.Draw(BackgroundOverlay, Bounds, BackgroundRepeatMode, BackgroundScale);
            }
        }

        public void AddChild(GuiElement element)
        {
            element.ParentElement = this;
            Children.Add(element);
            if (_initialised)
            {
                element.Init(_guiRenderer);
            }
            UpdateLayout();
        }

        public void RemoveChild(GuiElement element)
        {
            Children.Remove(element);
            element.ParentElement = null;
            UpdateLayout();
        }

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

        public bool TryFindDeepestChild(GuiElementPredicate predicate, out GuiElement childElement)
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

        public IEnumerable<TResult> ForEachChild<TResult>(Func<GuiElement, TResult> valueSelector)
        {
            if (HasChildren)
            {
                foreach (var child in Children.ToArray())
                {
                    yield return valueSelector(child);
                }
            }
        }

        public void ForEachChild(Action<GuiElement> childAction)
        {
            if (!HasChildren) return;

            foreach (var child in Children.ToArray())
            {
                childAction(child);
            }
        }
    }
}
