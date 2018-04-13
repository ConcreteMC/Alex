using System.Collections.Generic;
using System.Linq;
using Alex.API.Input.Listeners;
using Microsoft.Xna.Framework;

namespace Alex.API.Input
{
    public class InputManager
    {
        private Game Game;

        private Dictionary<PlayerIndex, PlayerInputManager> PlayerInputManagers { get; } = new Dictionary<PlayerIndex, PlayerInputManager>();

        public ICursorInputListener CursorInputListener { get; }

        public int PlayerCount => PlayerInputManagers.Count;

        public InputManager(Game game)
        {
            Game = game;

            var playerOne = GetOrAddPlayerManager(PlayerIndex.One);
            var mouseListener = new MouseInputListener(PlayerIndex.One);

            playerOne.AddListener(mouseListener);
            playerOne.AddListener(new KeyboardInputListener());

            CursorInputListener = mouseListener;
        }

        public PlayerInputManager GetOrAddPlayerManager(PlayerIndex playerIndex)
        {
            if (!PlayerInputManagers.TryGetValue(playerIndex, out var playerInputManager))
            {
                playerInputManager = new PlayerInputManager(playerIndex);

                PlayerInputManagers.Add(playerIndex, playerInputManager);
            }

            return playerInputManager;
        }

        public void Update()
        {
            foreach (var playerInputManager in PlayerInputManagers.Values.ToArray())
            {
                playerInputManager.Update();
            }
        }
    }
}
