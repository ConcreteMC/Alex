using System;
using Alex.Graphics.UI.Input.Listeners;

namespace Alex.Graphics.UI.Abstractions
{
	public interface IClickable
	{
		event EventHandler<MouseEventArgs> MouseDown;
		event EventHandler<MouseEventArgs> MouseUp;

		bool IsMouseDown { get; }

		void InvokeMouseDown(MouseEventArgs args);
		void InvokeMouseUp(MouseEventArgs args);

	}
}
