using System;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Gui.Events;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiControl : GuiContainer, IGuiControl
    {
        public event EventHandler<GuiCursorMoveEventArgs> CursorMove;
        public event EventHandler<GuiCursorEventArgs> CursorDown;
        public event EventHandler<GuiCursorEventArgs> CursorPressed;

        public event EventHandler<GuiCursorEventArgs> CursorEnter;
        public event EventHandler<GuiCursorEventArgs> CursorLeave;


        public GuiTexture2D DisabledBackground;
        public GuiTexture2D HighlightedBackground;
        public GuiTexture2D FocusedBackground;
        
        public virtual Color     HighlightOutlineColor { get; set; } = new Color(TextColor.Gray.ForegroundColor, 0.75f);
        public virtual Thickness HighlightOutlineThickness { get; set; } = Thickness.Zero;

        public virtual Color     FocusOutlineColor     { get; set; } = new Color(Color.White, 0.75f);
        public virtual Thickness FocusOutlineThickness { get; set; } = Thickness.Zero;

        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            DisabledBackground.TryResolveTexture(renderer);
            HighlightedBackground.TryResolveTexture(renderer);
            FocusedBackground.TryResolveTexture(renderer);
        }
        
        protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
        {
            base.OnDraw(graphics, gameTime);

            if (!Enabled)
            {
                graphics.FillRectangle(RenderBounds, DisabledBackground);
            }
            else 
            {
                if(Focused)
                {
                    graphics.FillRectangle(RenderBounds, FocusedBackground);
                    
                    if (FocusOutlineThickness != Thickness.Zero)
                    {
                        graphics.DrawRectangle(RenderBounds, FocusOutlineColor, FocusOutlineThickness, true);
                    }
                }

                if (Highlighted)
                {
                    graphics.FillRectangle(RenderBounds, HighlightedBackground);
                    
                    if (HighlightOutlineThickness != Thickness.Zero)
                    {
                        graphics.DrawRectangle(RenderBounds, HighlightOutlineColor, HighlightOutlineThickness, true);
                    }
                }
            }
        }

	    private bool _enabled = true;

	    public bool Enabled
	    {
		    get { return _enabled; }
		    set
		    {
			    _enabled = value;
			    OnEnabledChanged();
		    }
	    }

		protected virtual void OnEnabledChanged() { }

        public bool Focused { get; private set; }
        public bool Highlighted { get; private set; }

        public Keys AccessKey { get; set; } = Keys.None;
        public int TabIndex { get; set; } = -1;

        public bool Focus()
        {
            return FocusContext?.Focus(this) ?? false;
        }

        public void ClearFocus()
        {
            FocusContext?.ClearFocus(this);
        }

        public void InvokeHighlightActivate()
        {
            if (!Enabled) return;

            Highlighted = true;
            OnHighlightActivate();
        }
        protected virtual void OnHighlightActivate() { }

        public void InvokeHighlightDeactivate()
        {
            Highlighted = false;
            OnHighlightDeactivate();
        }
        protected virtual void OnHighlightDeactivate() { }


        public void InvokeFocusActivate()
        {
            if (!Enabled) return;

            Focused = true;
            OnFocusActivate();
        }
        protected virtual void OnFocusActivate() { }

        public void InvokeFocusDeactivate()
        {
            Focused = false;
            OnFocusDeactivate();
        }
        protected virtual void OnFocusDeactivate() { }


        public bool InvokeKeyInput(char character, Keys key)
        {
			return OnKeyInput(character, key);
        }

	    protected virtual bool OnKeyInput(char character, Keys key)
	    {
		    return false;
	    }


        public void InvokeCursorDown(Vector2 cursorPosition)
        {
            var pos = (cursorPosition - RenderPosition).ToPoint();
            CursorDown?.Invoke(this, new GuiCursorEventArgs(pos));

            OnCursorDown(pos);
        }
        protected virtual void OnCursorDown(Point cursorPosition) { }

        public void InvokeCursorPressed(Vector2 cursorPosition)
        {
            var pos = (cursorPosition - RenderPosition).ToPoint();
            CursorPressed?.Invoke(this, new GuiCursorEventArgs(pos));

            OnCursorPressed(pos);
        }
        protected virtual void OnCursorPressed(Point cursorPosition) { }

        public void InvokeCursorMove(Vector2 cursorPosition, Vector2 previousCursorPosition, bool isCursorDown)
        {
            var relativeNew = (cursorPosition - RenderPosition).ToPoint();
            var relativeOld = (previousCursorPosition - RenderPosition).ToPoint();

            CursorMove?.Invoke(this, new GuiCursorMoveEventArgs(relativeNew, relativeOld, isCursorDown));

            OnCursorMove(relativeNew, relativeOld, isCursorDown);
        }
        protected virtual void OnCursorMove(Point cursorPosition, Point previousCursorPosition, bool isCursorDown) { }


        public void InvokeCursorEnter(Vector2 cursorPosition)
        {
            var pos = (cursorPosition - RenderPosition).ToPoint();
            CursorEnter?.Invoke(this, new GuiCursorEventArgs(pos));
            
            OnCursorEnter(pos);
        }
        protected virtual void OnCursorEnter(Point cursorPosition) { }

        public void InvokeCursorLeave(Vector2 cursorPosition)
        {
            var pos = (cursorPosition - RenderPosition).ToPoint();
            CursorLeave?.Invoke(this, new GuiCursorEventArgs(pos));
            
            OnCursorLeave(pos);
        }
        protected virtual void OnCursorLeave(Point cursorPosition) { }

    }
}
