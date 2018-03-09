using System;
using System.Linq;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Input;
using Alex.Graphics.UI.Input.Listeners;
using Alex.Graphics.UI.Layout;
using Alex.Graphics.UI.Rendering;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI
{
    public class UiRoot : UiContainer
    {
        protected internal UiRenderer Renderer { get; }
     
        public UiRoot(UiRenderer renderer)
        {
            Renderer = renderer;
            Renderer.SizeChanged += RendererOnSizeChanged;
        }

        ~UiRoot()
        {
            Renderer.SizeChanged -= RendererOnSizeChanged;
        }

        private void RendererOnSizeChanged(object sender, EventArgs eventArgs)
        {
            UpdateLayoutInternal();
        }

        private IHoverable _hoveredElement;
        private IClickable _clickedElement;

        private IInputManager _input;
        
        public void Activate(IInputManager input)
        {
            _input = input;

            _input.MouseListener.MouseUp += OnMouseUp;
            _input.MouseListener.MouseDown += OnMouseDown;
            _input.MouseListener.MouseMove += OnMouseMove;
        }

        public void Deactivate()
        {
            if (_input == null) return;

            _input.MouseListener.MouseUp -= OnMouseUp;
            _input.MouseListener.MouseDown -= OnMouseDown;
            _input.MouseListener.MouseMove -= OnMouseMove;
        }

        protected override void OnUpdateLayout(UiElementLayoutParameters layout)
        {
            layout.MaxSize = layout.MinSize = new Point(Renderer.VirtualWidth, Renderer.VirtualHeight);
            layout.Size = layout.MinSize.ToVector2();

            base.OnUpdateLayout(layout);
        }


        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var element = FindElementAtPosition<IHoverable>(this, e.Position);

            if (_hoveredElement != element)
            {
                _hoveredElement?.InvokeMouseLeave(e);
                element?.InvokeMouseEnter(e);
            }
            else
            {
                element?.InvokeMouseMove(e);
            }

            _hoveredElement = element;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            var element = FindElementAtPosition<IClickable>(this, e.Position);

            if (_clickedElement != element)
            {
                _clickedElement?.InvokeMouseUp(e);
            }

            _clickedElement = element;
            element?.InvokeMouseDown(e);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            var element = FindElementAtPosition<IClickable>(this, e.Position);

            _clickedElement?.InvokeMouseUp(e);
            if (_clickedElement != element)
            {
                element?.InvokeMouseUp(e);
            }

            _clickedElement = null;
        }

        private static TUiElement FindElementAtPosition<TUiElement>(UiContainer container, Point position) where TUiElement : class
        {
            TUiElement element;

            var controls = container.Controls.ToArray();
            foreach (var control in controls)
            {
                if (control is UiContainer childContainer)
                {
                    element = FindElementAtPosition<TUiElement>(childContainer, position);
                    if (element != null)
                    {
                        return element;
                    }
                }

                element = control as TUiElement;
                if (element != null)
                {
                    if (control.LayoutParameters.Bounds.Contains(position))
                    {
                        return element;
                    }
                }
            }

            element = container as TUiElement;
            if (element != null)
            {
                if (container.LayoutParameters.Bounds.Contains(position))
                {
                    return element;
                }
            }

            return null;
        }

    }
}
