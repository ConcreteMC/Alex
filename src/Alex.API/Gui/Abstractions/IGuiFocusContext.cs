using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Gui
{
    public interface IGuiFocusContext
    {
        void HandleContextActive();
        void HandleContextInactive();

    }
}
