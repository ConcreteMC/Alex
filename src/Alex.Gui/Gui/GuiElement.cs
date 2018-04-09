using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui
{
    public class GuiElement
    {
        public GuiElement ParentElement { get; set; }

        public GuiScalar X { get; set; }
        public GuiScalar Y { get; set; }
        
        public GuiScalar Width { get; set; }
        public GuiScalar Height { get; set; }

        public IEnumerable<GuiElement> Children { get; } = new List<GuiElement>();
        public bool HasChildren => Children.Any();

        #region Layout

        public HorizontalAlignment HorizontalAlignment { get;set; }
        public VerticalAlignment VerticalAlignment { get; set; }

        #endregion

        #region Drawing

        public Texture2D Background { get; set; }

        public int AbsoluteWidth => Width.ToAbsolute(ParentElement?.AbsoluteWidth);
        public int AbsoluteHeight => Height.ToAbsolute(ParentElement?.AbsoluteHeight);

        public Point RelativePosition => new Point(X.ToAbsolute(ParentElement.AbsoluteWidth), Y.ToAbsolute(ParentElement.AbsoluteHeight));

        public Point AbsolutePosition => ParentElement.AbsolutePosition + RelativePosition;
        public Point AbsoluteSize => new Point(AbsoluteWidth, AbsoluteHeight);

        public Rectangle AbsoluteBounds => new Rectangle(AbsolutePosition, AbsoluteSize);

        #endregion

        public GuiElement()
        {

        }

        public void Init(IGuiRenderer renderer)
        {
            OnInit(renderer);

            ForEachChild(c => c.Init(renderer));
        }

        protected virtual void OnInit(IGuiRenderer renderer)
        {

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
            OnDraw(renderArgs);

            ForEachChild(c => c.Draw(renderArgs));
        }

        protected virtual void OnDraw(GuiRenderArgs renderArgs)
        {
            if (Background != null)
            {
                renderArgs.SpriteBatch.Draw(Background, AbsoluteBounds, Color.Black);
            }
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
