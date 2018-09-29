using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Alex.API.Gui.Layout;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui
{
    public interface IGuiElement
    {
        IGuiScreen Screen { get; }
        IGuiElement ParentElement { get; set; }
        IGuiFocusContext FocusContext { get; set; }

        bool HasChildren { get; }
        int X { set; }
        int Y { set; }
        

        Vector2 RenderPosition { get; }
        Size RenderSize { get; }
        Rectangle RenderBounds { get; }
        

        void Init(IGuiRenderer renderer, bool force = false);

        void Update(GameTime gameTime);

        void Draw(GuiSpriteBatch graphics, GameTime gameTime);

        void AddChild(IGuiElement element);
        void RemoveChild(IGuiElement element);
        
        #region Hierachy Transcending

        bool TryFindParent(GuiElementPredicate predicate, out IGuiElement parentElement);
        bool TryFindParentOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement parentElement) where TGuiElement : class, IGuiElement;

        bool TryTranscendChildren(GuiElementPredicate predicate, bool recurse = true);

        bool TryFindDeepestChild(GuiElementPredicate predicate, out IGuiElement childElement);
        bool TryFindDeepestChildOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement childElement) where TGuiElement : class, IGuiElement;
        
        void ForEachChild(Action<IGuiElement> element);
        IEnumerable<TResult> ForEachChild<TResult>(Func<IGuiElement, TResult> valueSelector);
        
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
        Alignment Anchor { get; set; }

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

        void InvalidateLayout(bool invalidateChildren = true);
        void InvalidateLayout(IGuiElement sender, bool invalidateChildren = true);



        #endregion
    }
}
