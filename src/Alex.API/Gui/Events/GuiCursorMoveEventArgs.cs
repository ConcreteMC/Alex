using Alex.API.Input;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Events
{
    public class GuiCursorMoveEventArgs : GuiCursorEventArgs
    {
		public Point PreviousCursorPosition { get; }

		public bool IsCursorDown { get; }

		internal GuiCursorMoveEventArgs(Point cursorPosition, Point previousCursorPosition, bool isCursorDown, MouseButton button) : base(cursorPosition, button)
		{
			PreviousCursorPosition = previousCursorPosition;
			IsCursorDown = isCursorDown;
		}
    }
}
