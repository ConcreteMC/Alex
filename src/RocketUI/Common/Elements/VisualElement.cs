using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using RocketUI.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RocketUI.Elements
{
    public delegate bool GuiElementPredicate(IVisualElement element);
    public delegate bool GuiElementPredicate<in TGuiElement>(TGuiElement element) where TGuiElement : class, IVisualElement;
    
    public partial class VisualElement : IVisualElement
    {
        private IGuiScreen _screen;
        private IVisualElement _parentElement;
        private IGuiFocusContext _focusContext;

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
        public IVisualElement ParentElement
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

        public virtual IGuiFocusContext FocusContext 
        {
            get { return _focusContext ?? ParentElement?.FocusContext ?? Screen; }
            set { _focusContext = value; }
        }

        protected List<IVisualElement> Children { get; } = new List<IVisualElement>();
        public bool HasChildren => Children.Any();

        public int ChildCount => Children.Count;
        protected internal IReadOnlyList<IVisualElement> AllChildren => Children;

        #region Drawing
        
        public virtual Vector2 RenderPosition => Position.ToVector2();
        public virtual Size RenderSize => Size;
        public virtual Rectangle RenderBounds => Bounds;
        
        public void Draw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            try
            {
                if (_initialised)
                {
                    OnDraw(graphics, gameTime);
                }
            }
            catch{}

            ForEachChild(c => c.Draw(graphics, gameTime));
        }

        
        #endregion
        
        private GuiManager _guiManager;
        private bool _initialised;
        
        protected GuiManager GuiManager => _guiManager;
        protected IGuiResourceProvider Resources => _guiManager.ResourceProvider;
        protected IGuiRenderer GuiRenderer => _guiManager.GuiRenderer;
        
        public VisualElement()
        {
            InitialiseStyledProperties();
        }

        #region Methods
        
        public void Init(GuiManager guiManager, bool force = false)
        {
            if (!_initialised || force)
            {
                _guiManager = guiManager;
                OnInit();
            }

            ForEachChild(c => c.Init(guiManager, force));

            _initialised = true;
        }

        protected virtual void OnInit()
        {
            //Resources.ResolveDefaultTextures(this);
            
            Background.TryResolveTexture(Resources);
            BackgroundOverlay.TryResolveTexture(Resources);
        }

        public void Update(GameTime gameTime)
        {
            UpdateStyle();

            OnUpdate(gameTime);

            ForEachChild(c => c.Update(gameTime));
        }
        protected virtual void OnUpdate(GameTime gameTime)
        {

        }
        

        public void AddChild(IVisualElement element)
        {
            if (element == this) return;
            if (element.ParentElement == this) return;
            if (Children.Contains(element)) return;

            element.ParentElement = this;
            Children.Add(element);
            if (_initialised)
            {
                element.Init(GuiManager);
            }
            OnChildAdded(element);
            InvalidateLayout();
        }

        public void RemoveChild(IVisualElement element)
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

        public bool TryFindParent(GuiElementPredicate predicate, out IVisualElement parentElement)
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

        public bool TryFindParentOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement parentElement) where TGuiElement : class, IVisualElement
        {

            var result = TryFindParent(e => e is TGuiElement e1 && predicate(e1), out IVisualElement element);

            parentElement = element as TGuiElement;
            return result;
        }

        public bool TryFindDeepestChild(GuiElementPredicate predicate, out IVisualElement childElement)
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

        public bool TryFindDeepestChildOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement childElement) where TGuiElement : class, IVisualElement
        {
            var result = TryFindDeepestChild(e => e is TGuiElement e1 && predicate(e1), out IVisualElement element);

            childElement = element as TGuiElement;
            return result;
        }

        public IEnumerable<TResult> ForEachChild<TResult>(Func<IVisualElement, TResult> valueSelector)
        {
            if (HasChildren)
            {
                foreach (var child in Children.ToArray())
                {
                    yield return valueSelector(child);
                }
            }
        }

        public void ForEachChild(Action<IVisualElement> childAction)
        {
            if (!HasChildren) return;

            foreach (var child in Children.ToArray())
            {
                childAction(child);
            }
        }

        private void ForEachChild<TElement>(Action<TElement> childAction) where TElement : class, IVisualElement
        {
            ForEachChild(c =>
            {
                if(c is TElement e) childAction(e);
            });
        }

        #endregion

        #region Event Handlers
        
        protected virtual void OnChildAdded(IVisualElement element) { }

        protected virtual void OnChildRemoved(IVisualElement element) { }

        protected virtual void OnScreenChanged(IGuiScreen previousScreen, IGuiScreen newScreen) { }

        protected virtual void OnParentElementChanged(IVisualElement parentElement) { }
        
        protected virtual void OnUpdateLayout() { }

        #endregion

    }
}

