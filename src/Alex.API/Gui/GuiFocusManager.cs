using System.Linq;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Input;
using Alex.API.Input.Listeners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui
{
    public class GuiFocusManager
    {
        private GuiControl _highlightedElement;
        private GuiControl _focusedElement;

        private GuiManager GuiManager { get; }
        private GraphicsDevice GraphicsDevice { get; }
        private InputManager InputManager { get; }

        private ICursorInputListener CursorInputListener => InputManager.CursorInputListener;

        private Viewport Viewport => GraphicsDevice.Viewport;

        private Vector2 _previousCursorPosition;
        public Vector2 CursorPosition { get; private set; }

        public GuiControl HighlightedElement
        {
            get => _highlightedElement;
            private set
            {
                if (_highlightedElement != null)
                {
                    _highlightedElement.IsHighlighted = false;
                }

                _highlightedElement = value;

                if (_highlightedElement != null)
                {
                    _highlightedElement.IsHighlighted = true;
                }
            }
        }

        public GuiControl FocusedElement
        {
            get => _focusedElement;
            private set
            {
                if (_focusedElement != null)
                {
                    _focusedElement.IsHighlighted = false;
                }

                _focusedElement = value;

                if (_focusedElement != null)
                {
                    _focusedElement.IsHighlighted = true;
                }
            }
        }


        public GuiFocusManager(GuiManager guiManager, InputManager inputManager, GraphicsDevice graphicsDevice)
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

        private void UpdateHighlightedElement()
        {
            var movePosition = CursorInputListener.GetCursorPositionDelta();

            //if (movePosition.X < 1 && movePosition.Y < 1) return;

            //var allElements = GuiManager.Screens.SelectMany(s => s.AllChildElements).ToArray();
            //var focusableControls = allElements.Where(e => e is GuiControl c && c.Enabled).Cast<GuiControl>().ToList();

            var rawCursorPosition = CursorInputListener.GetCursorPosition();

            CursorPosition = GuiManager.GuiRenderer.Unproject(rawCursorPosition);

            //var controlMatchingPosition = focusableControls.LastOrDefault(e => e.RenderBounds.Contains(CursorPosition));
            if (TryGetElementAt(CursorPosition, e => e is GuiControl c && c.Enabled, out var controlMatchingPosition))
            {
                HighlightedElement = controlMatchingPosition as GuiControl;
            }
            else
            {
                HighlightedElement = null;
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
                HighlightedElement.InvokeClick(CursorPosition);
            }

            var isDown = CursorInputListener.IsDown(InputCommand.Click);
            if (isDown)
            {
                HighlightedElement.InvokeCursorDown(CursorPosition);
            }

            if (CursorPosition.ToPoint() != _previousCursorPosition.ToPoint())
            {
                HighlightedElement.InvokeCursorMove(CursorPosition, _previousCursorPosition, isDown);
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

        private bool TryGetElementAt(Vector2 position, GuiElementPredicate predicate, out IGuiElement element)
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
