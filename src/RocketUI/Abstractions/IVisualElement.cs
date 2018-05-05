using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Xna.Framework;
using RocketUI.Elements;
using RocketUI.Graphics;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace RocketUI
{
    public interface IVisualElement
    {
        IGuiScreen Screen { get; }
        IVisualElement ParentElement { get; set; }
        IGuiFocusContext FocusContext { get; set; }

        bool HasChildren { get; }
        int X { set; }
        int Y { set; }
        

        Vector2 RenderPosition { get; }
        Size RenderSize { get; }
        Rectangle RenderBounds { get; }
        

        void Init(GuiManager guiManager, bool force = false);

        void Update(GameTime gameTime);

        void Draw(GuiSpriteBatch graphics, GameTime gameTime);

        void AddChild(IVisualElement element);
        void RemoveChild(IVisualElement element);
        
        #region Hierachy Transcending

        bool TryFindParent(GuiElementPredicate predicate, out IVisualElement parentElement);
        bool TryFindParentOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement parentElement) where TGuiElement : class, IVisualElement;

        bool TryTranscendChildren(GuiElementPredicate predicate, bool recurse = true);

        bool TryFindDeepestChild(GuiElementPredicate predicate, out IVisualElement childElement);
        bool TryFindDeepestChildOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement childElement) where TGuiElement : class, IVisualElement;
        
        void ForEachChild(Action<IVisualElement> element);
        IEnumerable<TResult> ForEachChild<TResult>(Func<IVisualElement, TResult> valueSelector);
        
        #endregion

        #region Layout Engine

        #region Properties
        int MinWidth  { get; set;}
        int MinHeight { get; set; }
        int MaxWidth  { get; set; }
        int MaxHeight { get; set; }

        int Width  { get; set; }
        int Height { get; set; }

        Thickness Margin  { get; }
        Thickness Padding { get; }

        AutoSizeMode AutoSizeMode { get; }
        Anchor Anchor { get; set; }

        #endregion


        #region Layout Calculation State Properties

        int LayoutOffsetX { get; }
        int LayoutOffsetY { get; }
        int LayoutWidth   { get; }
        int LayoutHeight  { get; }
        
        bool IsLayoutDirty { get; }

        Size PreferredSize { get; }
        Size PreferredMinSize { get; }
        Size PreferredMaxSize { get; }

        #endregion
        
        #region Calculated Properties

        Rectangle OuterBounds { get; }
        Rectangle Bounds { get; }
        Rectangle InnerBounds { get; }
        Point Position { get; }
        Size Size { get; }
        
        #endregion


        Size Measure(Size availableSize);
        void Arrange(Rectangle newBounds);

        void InvalidateLayout();
        void InvalidateLayout(IVisualElement sender);



        #endregion
    }
}
