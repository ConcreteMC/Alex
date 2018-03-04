using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Input.Listeners;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Input
{
    public class UiInputManager : IInputManager
    {

        public IMouseListener MouseListener { get; private set; }

        public UiInputManager()
        {
            MouseListener = new MouseListener();
        }
        

        public void Update(GameTime gameTime)
        {
            MouseListener.Update(gameTime);
        }

    }
}
