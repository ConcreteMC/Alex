using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Attributes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui
{
    public interface IGuiControl : IGuiElement
    {
        [DebuggerVisible] bool Enabled { get; }
        [DebuggerVisible] bool Focused { get; }
        [DebuggerVisible] bool Highlighted { get; }

        [DebuggerVisible] Keys AccessKey { get; set; }
        [DebuggerVisible] int TabIndex { get; set; }

        bool Focus();

        void InvokeHighlightActivate();
        void InvokeHighlightDeactivate();

        void InvokeFocusActivate();
        void InvokeFocusDeactivate();
        
        bool InvokeKeyInput(char character, Keys key);

        void InvokeCursorDown(Vector2 cursorPosition);
        void InvokeCursorPressed(Vector2 cursorPosition);
        void InvokeCursorMove(Vector2 cursorPosition, Vector2 previousCursorPosition, bool isCursorDown);

        void InvokeCursorEnter(Vector2 cursorPosition);
        void InvokeCursorLeave(Vector2 cursorPosition);
    }
}
