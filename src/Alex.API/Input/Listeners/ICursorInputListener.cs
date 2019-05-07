using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.API.Input.Listeners
{
    public interface ICursorInputListener : IInputListener
    {
        Vector2 GetCursorPositionDelta();
        Vector2 GetCursorPosition();
    }
}
