using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Input.Listeners;

namespace Alex.Gui.Common
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
