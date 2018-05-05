using Microsoft.Xna.Framework;

namespace RocketUI.Input.Listeners
{
    public interface ICursorInputListener : IInputListener
    {
        Vector2 GetCursorPositionDelta();
        Vector2 GetCursorPosition();
    }
}
