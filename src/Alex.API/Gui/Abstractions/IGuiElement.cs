using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui
{
    public interface IGuiElement
    {
        IGuiScreen Screen { get; }
        IGuiElement ParentElement { get; set; }

        bool HasChildren { get; }

        int LayoutOffsetX { get; set; }
        int LayoutOffsetY { get; set; }
        int LayoutWidth { get; set; }
        int LayoutHeight { get; set; }

        int X { get; set; }
        int Y { get; set; }
        
        int MinWidth  { get; }
        int MinHeight { get; }
        int MaxWidth  { get; }
        int MaxHeight { get; }

        int Width { get; }
        int Height { get; }

        Vector2 RenderPosition { get; }
        Point RenderSize { get; }
        Rectangle RenderBounds { get; }
        
        VerticalAlignment VerticalAlignment { get; set; }
        HorizontalAlignment HorizontalAlignment { get; set; }


        void Init(IGuiRenderer renderer);

        void UpdateLayout(bool updateChildren = true);

        void Update(GameTime gameTime);

        void Draw(GuiRenderArgs args);

        void AddChild(IGuiElement element);
        void RemoveChild(IGuiElement element);

        bool TryFindParent(GuiElementPredicate predicate, out IGuiElement parentElement);
        bool TryFindParentOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement parentElement) where TGuiElement : class, IGuiElement;

        bool TryTranscendChildren(GuiElementPredicate predicate, bool recurse = true);

        bool TryFindDeepestChild(GuiElementPredicate predicate, out IGuiElement childElement);
        bool TryFindDeepestChildOfType<TGuiElement>(GuiElementPredicate<TGuiElement> predicate, out TGuiElement childElement) where TGuiElement : class, IGuiElement;
        
        IEnumerable<TResult> ForEachChild<TResult>(Func<IGuiElement, TResult> valueSelector);
        void ForEachChild(Action<IGuiElement> element);
    }
}
