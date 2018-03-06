using System;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Input.Listeners;

namespace Alex.Graphics.UI.Controls
{
	public class UiControl : UiContainer, IHoverable, IClickable
	{
		public event EventHandler<MouseEventArgs> MouseEnter;
		public event EventHandler<MouseEventArgs> MouseMove;
		public event EventHandler<MouseEventArgs> MouseLeave;
		public event EventHandler<MouseEventArgs> MouseDown;
		public event EventHandler<MouseEventArgs> MouseUp;

		public bool IsMouseOver { get; private set; }
		public bool IsMouseDown { get; private set; }

		public void InvokeMouseEnter(MouseEventArgs args)
		{
			IsMouseOver = true;
			OnMouseEnter(args);
			MouseEnter?.Invoke(this, args);
		}

		public void InvokeMouseMove(MouseEventArgs args)
		{
			OnMouseMove(args);
			MouseMove?.Invoke(this, args);
		}

		public void InvokeMouseLeave(MouseEventArgs args)
		{
			IsMouseOver = false;
			OnMouseLeave(args);
			MouseLeave?.Invoke(this, args);
		}

		public void InvokeMouseDown(MouseEventArgs args)
		{
			IsMouseDown = true;
			OnMouseDown(args);
			MouseDown?.Invoke(this, args);
		}

		public void InvokeMouseUp(MouseEventArgs args)
		{
			IsMouseDown = false;
			OnMouseUp(args);
			MouseUp?.Invoke(this, args);
		}

		protected virtual void OnMouseEnter(MouseEventArgs args) { }
		protected virtual void OnMouseMove(MouseEventArgs args) { }
		protected virtual void OnMouseLeave(MouseEventArgs args) { }
		protected virtual void OnMouseDown(MouseEventArgs args) { }
		protected virtual void OnMouseUp(MouseEventArgs args) { }
	}
}
