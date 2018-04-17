using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiControl : GuiContainer
    {

        public bool Enabled { get; set; } = true;

        private bool _isHighlighted = false;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted == value) return;
                _isHighlighted = value;

                if(_isHighlighted)
                    InvokeHighlighted();
            }
        }

        private bool _isFocused = false;
        public bool IsFocused
        {
            get => _isFocused;
            set
            {
                if (_isFocused == value) return;
                _isFocused = value;

                if(_isFocused)
                    InvokeFocused();
            }
        }

        public GuiTextures? HighlightedBackgroundTexture { get; set; }
        public GuiTextures? FocusedBackgroundTexture { get; set; }

        public TextureSlice2D HighlightedBackground { get; set; }
        public TextureSlice2D FocusedBackground { get; set; }

        public virtual Color HighlightOutlineColor { get; set; } = Color.White;
        public virtual Thickness HighlightOutlineThickness { get; set; } = Thickness.One;
        public virtual Color     FocusOutlineColor     { get; set; } = Color.White;
        public virtual Thickness FocusOutlineThickness { get; set; } = Thickness.One;
        
        protected override void OnInit(IGuiRenderer renderer)
        {
            base.OnInit(renderer);
            
            if (HighlightedBackgroundTexture.HasValue)
            {
                HighlightedBackground = renderer.GetTexture(HighlightedBackgroundTexture.Value);
            }
            if (FocusedBackgroundTexture.HasValue)
            {
                FocusedBackground = renderer.GetTexture(FocusedBackgroundTexture.Value);
            }
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);


            if (IsFocused)
            {
                Background = FocusedBackground ?? (IsHighlighted ? HighlightedBackground ?? DefaultBackground : DefaultBackground);
            }
            else
            {
                if (IsHighlighted)
                {
                    Background = HighlightedBackground ?? DefaultBackground;
                }
                else
                {
                    Background = DefaultBackground;
                }
            }
        }

        protected override void OnDraw(GuiRenderArgs args)
        {
            base.OnDraw(args);


            if (IsFocused && FocusOutlineThickness.Size() > 0)
            {
                var outlineBounds = RenderBounds;
                outlineBounds.Inflate(1f,1f);
                args.DrawRectangle(outlineBounds, FocusOutlineColor, FocusOutlineThickness);
            }
            else if (IsHighlighted && HighlightOutlineThickness.Size() > 0)
            {
                var outlineBounds = RenderBounds;
                outlineBounds.Inflate(1f,1f);
                args.DrawRectangle(outlineBounds, HighlightOutlineColor, HighlightOutlineThickness);
            }
            
        }

        #region Control Cursor Events
        
        public void InvokeHighlighted()
        {
            OnHighlighted();
        }
        protected virtual void OnHighlighted() {}


        public void InvokeFocused()
        {
            OnFocused();
        }
        protected virtual void OnFocused() {}


        public void InvokeClick(Vector2 cursorPosition)
        {
            var relative = cursorPosition - RenderPosition;

            OnClick(relative);
        }
        protected virtual void OnClick(Vector2 relativePosition) { }


        public void InvokeCursorDown(Vector2 cursorPosition)
        {
            var relative = cursorPosition - RenderPosition;

            OnCursorDown(relative);
        }
        protected virtual void OnCursorDown(Vector2 relativePosition) { }

        
        public void InvokeCursorMove(Vector2 newPosition, Vector2 oldPosition, bool isCursorDown)
        {
            var relativeNew = newPosition - RenderPosition;
            var relativeOld = oldPosition - RenderPosition;

            OnCursorMove(relativeNew, relativeOld, isCursorDown);
        }
        protected virtual void OnCursorMove(Vector2 relativeNewPosition, Vector2 relativeOldPosition, bool isCursorDown) { }

        #endregion

        #region Control Keyboard Events

        public void InvokeKeyInput(char character, Keys key)
        {
            OnKeyInput(character, key);
        }
        protected virtual void OnKeyInput(char character, Keys key) {}

        #endregion

    }
}
