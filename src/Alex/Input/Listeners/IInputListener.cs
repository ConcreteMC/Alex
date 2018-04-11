using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.Input
{
    public interface IInputListener
    {
        PlayerIndex PlayerIndex { get; }

        void Update();

        bool IsDown(InputCommand command);
        bool IsUp(InputCommand command);
        bool IsBeginPress(InputCommand command);
        bool IsPressed(InputCommand command);

    }
}
