using System;
using Microsoft.Xna.Framework;

namespace RocketUI
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
