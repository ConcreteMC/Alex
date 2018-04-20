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
    
    public partial class GuiElement : IGuiElement
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
        public Vector2? BackgroundScale { get; set; }
        
        public Color? BackgroundOverlayColor
        {
            get => _backgroundOverlayColor;
            set => _backgroundOverlayColor = value;
        }

        public virtual Vector2 RenderPosition => Position.ToVector2();
        public virtual Size RenderSize => Size;
        public virtual Rectangle RenderBounds => Bounds;
        
        public void Draw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            // Init matrix
            OnDraw(graphics, gameTime);

            ForEachChild(c => c.Draw(graphics, gameTime));
        }

        protected virtual void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            if (Background != null)
            {
                if (BackgroundScale.HasValue)
                {
                    graphics.FillRectangle(RenderBounds, Background, BackgroundRepeatMode, BackgroundScale.Value);
                }
                else
                {
                    graphics.FillRectangle(RenderBounds, Background, BackgroundRepeatMode);
                }
            }

            if (BackgroundOverlayColor.HasValue)
            {
                graphics.FillRectangle(RenderBounds, BackgroundOverlayColor.Value);
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
        
        public void Init(IGuiRenderer renderer, bool force = false)
        {
            if (!_initialised || force)
            {
                OnInit(renderer);
                _guiRenderer = renderer;
            }

            ForEachChild(c => c.Init(renderer, force));

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

    }
}

