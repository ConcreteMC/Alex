using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Events
{
    public class GuiCursorMoveEventArgs : GuiCursorEventArgs
    {
		public Point PreviousCursorPosition { get; }

		public bool IsCursorDown { get; }

		internal GuiCursorMoveEventArgs(Point cursorPosition, Point previousCursorPosition, bool isCursorDown) : base(cursorPosition)
		{
			PreviousCursorPosition = previousCursorPosition;
			IsCursorDown = isCursorDown;
		}
    }
}
