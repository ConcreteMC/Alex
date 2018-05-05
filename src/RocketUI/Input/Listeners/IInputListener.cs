using Microsoft.Xna.Framework;

namespace RocketUI.Input.Listeners
{
    public interface IInputListener
    {
        PlayerIndex PlayerIndex { get; }

        void Update(GameTime gameTime);

        bool IsDown(string command);
        bool IsUp(string command);
        bool IsBeginPress(string command);
        bool IsPressed(string command);

    }
}
