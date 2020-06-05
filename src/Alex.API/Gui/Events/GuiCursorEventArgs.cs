using System;
using Alex.API.Input;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Events
{
    public class GuiCursorEventArgs : EventArgs
    {
		public Point CursorPosition { get; }
		public MouseButton Button { get; }

		internal GuiCursorEventArgs(Point cursorPosition, MouseButton button)
		{
			CursorPosition = cursorPosition;
			Button = button;
		}
    }
}
