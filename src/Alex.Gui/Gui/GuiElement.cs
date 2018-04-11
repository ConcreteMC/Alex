using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.Textures;
using Alex.Graphics.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui
{
    public class GuiElement
    {
        public GuiElement ParentElement { get; set; }
        
        public virtual int X  { get; set; } = 0;
        public virtual int Y { get; set; } = 0;

        public virtual int LayoutOffsetX { get; set; } = 0;
        public virtual int LayoutOffsetY { get; set; } = 0;

        protected List<GuiElement> Children { get; } = new List<GuiElement>();
        public bool HasChildren => Children.Any();

        protected Color DebugColor { get; set; } = Color.Red;

        #region Layout

        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.None;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.None;

        #endregion

        #region Drawing

        public NinePatchTexture Background { get; set; }

        private int _width = -1;
        private int _height = -1;

        public virtual int Width
        {
            get => _width < 0 ? (HasChildren ? Math.Abs(Children.Max(c => c.Bounds.Right) - Children.Min(c => c.Bounds.Left)) : 0) : _width;
            set => _width = value;
        }
        public virtual int Height
        {
            get => _height < 0 ? (HasChildren ? Math.Abs(Children.Max(c => c.Bounds.Bottom) - Children.Min(c => c.Bounds.Top)) : 0) : _height;
            set => _height = value;
        }
        
        public virtual Vector2   Position => (ParentElement?.Position ?? Vector2.Zero) + new Vector2(LayoutOffsetX, LayoutOffsetY) + new Vector2(X, Y);
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
                LayoutOffsetY = (int)((ParentElement.Height - Height) / 2f);
            }
            else if (VerticalAlignment == VerticalAlignment.Bottom)
            {
                LayoutOffsetY = (int) (ParentElement.Height - Height);
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
                LayoutOffsetX = (int)((ParentElement.Width - Width) / 2f);
            }
            else if (HorizontalAlignment == HorizontalAlignment.Right)
            {
                LayoutOffsetX = (int) (ParentElement.Width - Width);
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
            OnDraw(renderArgs);

            ForEachChild(c => c.Draw(renderArgs));
        }

        protected virtual void OnDraw(GuiRenderArgs args)
        {
            args.DrawRectangle(Bounds, DebugColor);

            if (Background != null)
            {
                args.DrawNinePatch(Bounds, Background, TextureRepeatMode.Stretch);
            }
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
