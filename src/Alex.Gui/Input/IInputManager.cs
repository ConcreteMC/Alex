using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Input.Listeners;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Input
{
    public interface IInputManager
    {

        IMouseListener MouseListener { get; }

        void Update(GameTime gameTime);
    }
}
