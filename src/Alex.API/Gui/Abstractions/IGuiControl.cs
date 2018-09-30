using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui
{
    public interface IGuiControl : IGuiElement
    {
        bool Enabled { get; }
        bool Focused { get; }
        bool Highlighted { get; }

        Keys AccessKey { get; set; }
        int TabIndex { get; set; }

        bool Focus();

        void InvokeHighlightActivate();
        void InvokeHighlightDeactivate();

        void InvokeFocusActivate();
        void InvokeFocusDeactivate();
        
        void InvokeKeyInput(char character, Keys key);

        void InvokeCursorDown(Vector2 cursorPosition);
        void InvokeCursorPressed(Vector2 cursorPosition);
        void InvokeCursorMove(Vector2 cursorPosition, Vector2 previousCursorPosition, bool isCursorDown);

        void InvokeCursorEnter(Vector2 cursorPosition);
        void InvokeCursorLeave(Vector2 cursorPosition);
    }
}
