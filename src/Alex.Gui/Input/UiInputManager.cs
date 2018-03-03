using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Input.Listeners;
using Microsoft.Xna.Framework;

namespace Alex.Gui.Input
{
    public class UiInputManager
    {
        private Game Game { get; }

        public MouseListener MouseListener { get; }

        public UiInputManager(Game game)
        {
            Game = game;

            MouseListener = new MouseListener();
        }

        public void Update(GameTime gameTime)
        {
            MouseListener.Update(gameTime);
        }

    }
}
