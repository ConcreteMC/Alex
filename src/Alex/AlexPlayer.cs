using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Input;
using Microsoft.Xna.Framework;

namespace Alex
{
    public class AlexPlayer : GameComponent
    {
        private readonly Alex Alex;

        public PlayerIndex PlayerIndex { get; }

        public PlayerInputManager InputManager { get; }

        public AlexPlayer(Alex alex, PlayerIndex playerIndex) : base(alex)
        {
            Alex = alex;
            PlayerIndex = playerIndex;

            InputManager = alex.InputManager.GetOrAddPlayerManager(playerIndex);
        }

        public override void Update(GameTime gameTime)
        {
            // no. bad kenny.
            //InputManager.Update();

            base.Update(gameTime);
        }
    }
}
