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
        
        public virtual int Width  { get; set; } = 0;
        public virtual int Height { get; set; } = 0;

        protected List<GuiElement> Children { get; } = new List<GuiElement>();
        public bool HasChildren => Children.Any();

        #region Layout

        public HorizontalAlignment HorizontalAlignment { get;set; }
        public VerticalAlignment VerticalAlignment { get; set; }

        #endregion

        #region Drawing

        public Texture2D Background { get; set; }
        
        public virtual int X { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

        public virtual Vector2   Position => (ParentElement?.Position ?? Vector2.Zero) + new Vector2(X, Y);
        public         Rectangle Bounds   => new Rectangle((int) Position.X, (int) Position.Y, Width, Height);

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

        public void UpdateLayout()
        {
            OnUpdateLayout();

            ForEachChild(c => c.UpdateLayout());
        }

        protected virtual void OnUpdateLayout()
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

        protected virtual void OnDraw(GuiRenderArgs args)
        {
            DrawRectangle(args, Bounds, Color.Red);

            if (Background != null)
            {
                args.SpriteBatch.Draw(Background, Bounds, Color.White);
            }
        }
        

        public void DrawRectangle(GuiRenderArgs args, Rectangle bounds, Color color, int thickness = 1)
        {
            var texture = new Texture2D(args.Graphics, 1, 1, false, SurfaceFormat.Color);
            texture.SetData(new Color[] {color});

            // Top
            args.SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);

            // Right
            args.SpriteBatch.Draw(texture, new Rectangle(bounds.X + bounds.Width - thickness, bounds.Y, thickness, bounds.Height),
                             color);

            // Bottom
            args.SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y + bounds.Height - thickness, bounds.Width, thickness),
                             color);

            // Left
            args.SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
        }

        public void AddChild(GuiElement element)
        {
            element.ParentElement = this;
            Children.Add(element);
            UpdateLayout();
        }

        public void RemoveChild(GuiElement element)
        {
            Children.Remove(element);
            element.ParentElement = null;
            UpdateLayout();
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
