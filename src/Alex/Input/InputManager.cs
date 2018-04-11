using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.Input
{
    public class InputManager
    {
        private Alex Alex;

        private Dictionary<PlayerIndex, IInputListener> PlayerListeners { get; } = new Dictionary<PlayerIndex, IInputListener>();

        public int PlayerCount { get; } = 1;

        public InputManager(Alex alex)
        {
            Alex = alex;

        }

        public void AddPlayer(PlayerIndex playerIndex)
        {
            //PlayerListeners.Add();
        }

        public void Update()
        {

        }

    }
}
