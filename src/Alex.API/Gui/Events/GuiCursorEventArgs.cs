using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Events
{
    public class GuiCursorEventArgs : EventArgs
    {
		public Point CursorPosition { get; }

		internal GuiCursorEventArgs(Point cursorPosition)
		{
			CursorPosition = cursorPosition;
		}
    }
}
