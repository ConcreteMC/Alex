using Microsoft.Xna.Framework;

namespace Alex.API.Input.Listeners
{
    public interface IInputListener
    {
        PlayerIndex PlayerIndex { get; }

        void Update(GameTime gameTime);

        bool IsDown(InputCommand command);
        bool IsUp(InputCommand command);
        bool IsBeginPress(InputCommand command);
        bool IsPressed(InputCommand command);

    }
}
