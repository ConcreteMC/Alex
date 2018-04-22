using System;
using System.Linq;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Input;
using Alex.API.Input.Listeners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui
{
    public class GuiFocusHelper
    {
        private GuiManager GuiManager { get; }
        private GraphicsDevice GraphicsDevice { get; }
        private InputManager InputManager { get; }

        private ICursorInputListener CursorInputListener => InputManager.CursorInputListener;

        private Viewport Viewport => GraphicsDevice.Viewport;

        private Vector2 _previousCursorPosition;
        public Vector2 CursorPosition { get; private set; }

        
        private IGuiControl _highlightedElement;
        private IGuiControl _focusedElement;

        public IGuiControl HighlightedElement
        {
            get => _highlightedElement;
            set
            {
                _highlightedElement?.InvokeHighlightDeactivate();
                _highlightedElement = value;
                _highlightedElement?.InvokeHighlightActivate();
            }
        }
        public IGuiControl FocusedElement
        {
            get => _focusedElement;
            set
            {
                _focusedElement?.InvokeFocusDeactivate();
                _focusedElement = value;
                _focusedElement?.InvokeFocusActivate();
            }
        }

        private IGuiFocusContext _activeFocusContext;
        public IGuiFocusContext ActiveFocusContext
        {
            get => _activeFocusContext;
            set
            {
                if (_activeFocusContext == value) return;

                _activeFocusContext?.HandleContextInactive();
                _activeFocusContext = value;
                _activeFocusContext?.HandleContextActive();

            }
        }

        public GuiFocusHelper(GuiManager guiManager, InputManager inputManager, GraphicsDevice graphicsDevice)
        {
            GuiManager = guiManager;
            InputManager = inputManager;
            GraphicsDevice = graphicsDevice;
        }
        

        public void Update(GameTime gameTime)
        {
            UpdateHighlightedElement();
            UpdateInput();
        }

        public void OnTextInput(object sender, TextInputEventArgs args)
        {
            FocusedElement?.InvokeKeyInput(args.Character, args.Key);
        }

        private void UpdateHighlightedElement()
        {
            var rawCursorPosition = CursorInputListener.GetCursorPosition();

            var cursorPosition = GuiManager.GuiRenderer.Unproject(rawCursorPosition);

            if (Vector2.DistanceSquared(rawCursorPosition, _previousCursorPosition) >= 1)
            {
                _previousCursorPosition = CursorPosition;
                CursorPosition = cursorPosition;
            }

            IGuiControl newHighlightedElement = null;

            if (TryGetElementAt(CursorPosition, e => e is IGuiControl c && c.Enabled, out var controlMatchingPosition))
            {
                newHighlightedElement = controlMatchingPosition as IGuiControl;
            }

            if (newHighlightedElement != HighlightedElement)
            {
                HighlightedElement?.InvokeCursorLeave(CursorPosition);
                HighlightedElement = newHighlightedElement;
                HighlightedElement?.InvokeCursorEnter(CursorPosition);
            }
        }

        private void UpdateInput()
        {
            if (HighlightedElement == null) return;

            if (CursorInputListener.IsBeginPress(InputCommand.Click))
            {
                FocusedElement = HighlightedElement;
            }

            if (HighlightedElement == FocusedElement && CursorInputListener.IsPressed(InputCommand.Click))
            {
                HighlightedElement?.InvokeCursorPressed(CursorPosition);
            }

            var isDown = CursorInputListener.IsDown(InputCommand.Click);
            if (isDown)
            {
                HighlightedElement?.InvokeCursorDown(CursorPosition);
            }

            if (CursorPosition != _previousCursorPosition)
            {
                HighlightedElement?.InvokeCursorMove(CursorPosition, _previousCursorPosition, isDown);
            }
        }

        private bool TryFindNextControl(Vector2 scanVector, out IGuiElement nextControl)
        {
            Vector2 scan = CursorPosition + scanVector;

            while (Viewport.Bounds.Contains(scan))
            {
                if (TryGetElementAt(scan, e => true, out var matchedElement))
                {
                    if (matchedElement != HighlightedElement)
                    {
                        nextControl = matchedElement;
                        return true;
                    }
                }

                scan += scanVector;
            }

            nextControl = null;
            return false;
        }

        public bool TryGetElementAt(Vector2 position, GuiElementPredicate predicate, out IGuiElement element)
        {
            foreach (var screen in GuiManager.Screens.ToArray().Reverse())
            {
                if (screen.TryFindDeepestChild(e => e.RenderBounds.Contains(position) && predicate(e), out var matchedChild))
                {
                    element = matchedChild;
                    return true;
                }
            }

            element = null;
            return false;
        }
    }
}
